using System;
using System.Xml;

namespace MuseSynthesis
{   // Class that does calculations for lead notes from input, and writes them to the ScoreWriter
    internal class LeadNote : NoteRest
    {
        ScoreWriter writer; // So that we can access more general settings here

        int drum; // What drum pitch to use
        int tupletdiv; // How many notes per 128th note
        double notevalue;

        string note; // Name of represented note
        string notevaluetext; // Note value in text
        double freq; // Frequency to be obtained
        double tempo; // The eventual tempo command that needs to be set
        int length; // How many notes should be written, rounded down to tuplets in the place of one 128th note

        bool portamento; // Whether to glide to another tone
        string targetnote; // Only to display in the tempotext
        double mintempo, // We always need to calculate the length based on the min tempo, because we can add more tuples later...
            maxtempo;    // but not remove them
        int minlength;

        public LeadNote(ScoreWriter writer, string note, string value, XmlNode effects)
        {
            this.writer = writer;
            this.note = note;
            drum = writer.drums[0];
            tupletdiv = 4; // Later to be variable
            notevaluetext = value; // We access this again in ActivateEffects
            notevalue = ReadNoteValue(notevaluetext);

            freq = CalcFreq(); // Calculate required frequency of sound
            tempo = CalcTempo();
            length = CalcLength();
            minlength = length; mintempo = tempo; maxtempo = tempo; // Will be reset in case of portamento

            ActivateEffects(effects);
        }

        // Writes the required XML for this lead note
        public void Write()
        {
            XmlDocument creator = new XmlDocument(); // Elements should be created in the context of an XmlDocument
            XmlElement settempo = creator.CreateElement("Tempo");
            XmlElement tempotag = creator.CreateElement("tempo");
            tempotag.InnerText = (tempo / 60).ToString(System.Globalization.CultureInfo.InvariantCulture); // In MuseScore, 120 bpm is internally represented as 2
            XmlElement texttag = creator.CreateElement("text");
            string tempotext = note; // We display the note played instead of the tempo, as that's much more useful for a reader
            if (portamento)
                tempotext += " → " + targetnote;
            texttag.InnerText = tempotext; 
            settempo.AppendChild(tempotag);
            settempo.AppendChild(texttag);
            writer.AppendChild(settempo);

            // Calculating the value that the individual notes have
            int log2div = (int)Math.Log2(tupletdiv); // We have to round down to a power of 2
            int notevaluenumber = 128 * (int)Math.Pow(2, log2div);
            string notevalue = notevaluenumber.ToString() + "th";

            // The Tuplets object will handle the rest of the writing for us
            Tuplets tuplets = new Tuplets(writer, minlength, tempo, drum, tupletdiv, notevalue, portamento, mintempo, maxtempo, true);
            tuplets.Write();
        }

        // Checks effects node (might not exist) to see what effects to apply, and sets them up
        private void ActivateEffects(XmlNode effects)
        {
            portamento = false;
            if (effects == null)
                return;
            XmlNode portnode = effects.SelectSingleNode("portamento");
            if (portnode != null)
            {
                portamento = true;
                targetnote = portnode.SelectSingleNode("goalnote").InnerText;
                LeadNote goalnote = new LeadNote(writer, targetnote, notevaluetext, null); // Creating a new LeadNote is an easy way to calculate the goal tempo and length
                double goaltempo = goalnote.tempo;
                if (goaltempo > tempo) // Portamento upwards
                {
                    mintempo = tempo;
                    maxtempo = goaltempo;
                    minlength = length;
                }
                else // Portamento downwards
                {
                    mintempo = goaltempo;
                    maxtempo = tempo;
                    minlength = goalnote.length;
                }
            }
            return;
        }

        // Calculates frequency that note should have (currently always according to equal temperament)
        private double CalcFreq()
        {
            double a0 = writer.a4tuning / Math.Pow(2,4); // Let's start from the bottom
            double c1 = a0 * Math.Pow(2, (double)3 / 12); // However, it's better to count from C because it's where the octave gets incremented
            char rootnotename = note[0];
            int noteoctave = (int)Char.GetNumericValue(note[note.Length - 1]);
            string alteration = note.Substring(1,note.Length - 2); // Take everything except the root note and the octave for sharps and flats
            int noteindex; // How far from C are we?
            switch (rootnotename)
            {
                case 'C':
                    noteindex = 0;
                    break;
                case 'D':
                    noteindex = 2;
                    break;
                case 'E':
                    noteindex = 4;
                    break;
                case 'F':
                    noteindex = 5;
                    break;
                case 'G':
                    noteindex = 7;
                    break;
                case 'A':
                    noteindex = 9;
                    break;
                case 'B':
                    noteindex = 11;
                    break;
                default:
                    throw new LeadNoteException ("You entered an invalid note name: " + rootnotename);
            }
            switch (alteration)
            {
                case "#":
                    noteindex++;
                    break;
                case "x":
                    noteindex += 2;
                    break;
                case "b":
                    noteindex--;
                    break;
                case "bb":
                    noteindex -= 2;
                    break;
                case "":
                    break;
                default:
                    throw new LeadNoteException("You entered an invalid alteration: " + alteration);
            }
            double frequencyratio = Math.Pow(2, (double)noteindex / 12);
            return c1 * frequencyratio * Math.Pow(2, noteoctave - 1); // -1 because we've decided to start counting from c1, not c0
        }
        
        // Calculates the tempo command to be set
        private double CalcTempo()
        {
            double notespmin = freq * 60; // From vibrations per second to notes per minute
            double notespbeat = tupletdiv * (128 / 4); // Amount of notes per beat
            double bpm = notespmin / notespbeat;
            return bpm;
        }

        // Calculates how many tuples are needed, based on the actual tempo and the effective tempo
        private int CalcLength()
        {
            double ratio = tempo / writer.tempo; // Ratio of actual bpm versus perceived bpm
            double notespmeas = tupletdiv * 128; // Amount of notes per measure
            double notes = notespmeas * ratio * notevalue; // Exact amount of notes needed
            int tuplets = (int)Math.Round(notes / tupletdiv);
            return tuplets;
        }
    }
}

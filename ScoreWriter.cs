using System.Xml;
using System.Collections;
using System;

namespace MuseSynthesis
{
    // Reads the input XML and writes the output XML
    internal class ScoreWriter
    {
        private XmlDocument input; // Document to read from
        private XmlDocument output; // Document to write to
        public int voices { get; private set; } // Amount of voices
        public int tempo { get; private set; } // Current tempo
        public int a4tuning { get; private set; } // Frequency at which A4 should sound
        public int[] drums { get; private set; } // The drum sounds for each voice to use
        private int songlength; // Keeps track of the total length of the notes and rests written, to decide on the time signature

        public bool displaytempos { get; private set; } // Whether to let MuseScore display the extra tempo commands; looks messy, but better for debugging

        public ScoreWriter(XmlDocument output, XmlDocument input)
        {
            this.output = output;
            this.input = input;
            voices = 2; // Program should later support multiple voices
            tempo = 120; // Default effective tempo; can be changed by command
            a4tuning = 440; // Tuning of A4 in Hertz; can be changed by command
            drums = new int[voices];
            drums[0] = 41; // Default drum; can be changed by command

            SetPreferences();
        }

        // Write the score from the input
        public void WriteScore()
        {
            songlength = 0;

            // We will read through all commands from top to bottom
            XmlNode scoreinput = input.SelectSingleNode("/museSynthesis/score");
            IEnumerator inputenumerator = scoreinput.GetEnumerator();
            while (inputenumerator.MoveNext())
            {
                if (inputenumerator.Current.GetType() == typeof(System.Xml.XmlComment)) // Don't try to process comments
                    continue;
                XmlElement current = (XmlElement)inputenumerator.Current;
                switch (current.Name) // Take action depending on tag name
                {
                    case "tempo":
                        tempo = int.Parse(current.InnerText);
                        break;

                    case "tuning":
                        a4tuning = int.Parse(current.InnerText);
                        break;

                    case "drum":
                        int voice = int.Parse(current.GetAttribute("voice"));
                        drums[voice] = int.Parse(current.InnerText);
                        break;

                    case "leadnote":
                        {
                            string note = current.SelectSingleNode("note").InnerText;
                            string value = current.SelectSingleNode("value").InnerText;
                            XmlNode effects = current.SelectSingleNode("effects");
                            XmlNode harmony = current.SelectSingleNode("harmony");
                            LeadNote leadnote = new LeadNote(this, note, value, effects, harmony);
                            leadnote.Write();
                            break;
                        }

                    case "leadrest":
                        { 
                        string value = current.SelectSingleNode("value").InnerText;
                        LeadRest leadrest = new LeadRest(this, value);
                        leadrest.Write();
                        break;
                        }

                    default:
                        throw new ScoreWriterException("You used a non-supported node name: " + current.Name);
                }
            }

            // Decide on time signature
            int quarternotes = songlength / 32 + 1; // 32 128th notes fit in one quarter note
            for (int voice = 0; voice < voices; voice++) // Set the time signature for each voice (0-indexed as usual)
            {
                XmlNode timesig = output.SelectSingleNode("/museScore/Score/Staff[@id='" + (voice + 1) + "']/Measure/voice/TimeSig");
                timesig.SelectSingleNode("sigN").InnerText = quarternotes.ToString();
            }
        }

        // Adds element to the output
        public void AppendChild(XmlElement appendum)
        {
            XmlNode child = output.ImportNode(appendum, true); // Nodes must be created in the right context, but we can import it 
            XmlNode outputnode = output.SelectSingleNode("/museScore/Score/Staff/Measure/voice"); // Navigate to the right place to write to
            outputnode.AppendChild(child);
        }

        internal void SetPreferences()
        {
            // Set default preferences
            displaytempos = false;

            XmlNodeList preferences = input.SelectNodes("/museSynthesis/preference");
            foreach (XmlElement preferencetag in preferences)
            {
                if (preferencetag.InnerText == "true")
                { 
                    string preference = preferencetag.GetAttribute("name");
                    switch (preference)
                    {
                        case "displaytempos":
                            displaytempos = true;
                            break;

                        default:
                            Console.WriteLine("You specified a non-supported preference: " + preference);
                            break;
                    }
                }
            }
        }

        // Update specified metaTags
        public void UpdateMetaTags()
        {
            XmlNodeList metatags = output.SelectNodes("/museScore/Score/metaTag");

            for (int tag = 0; tag < metatags.Count; tag++) // Go through all metatags listed in default.xml
            {
                XmlElement metatag = (XmlElement)metatags[tag];
                string setting = metatag.GetAttribute("name");
                XmlNodeList test = input.SelectNodes("/museSynthesis/metaTag");
                XmlNode target = input.SelectSingleNode("/museSynthesis/metaTag[@name='"+setting+"']"); // Select that metatag if it exists
                if (target != null)
                {
                    metatags[tag].InnerText = target.InnerText;
                }
            }
        }

        // Increases song length in 128th notes with the specified increase
        public void CountIncrease(int increase)
        {
            songlength += increase;
        }
    }
}

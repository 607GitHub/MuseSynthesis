using System;
using System.Xml;

namespace MuseSynthesis
{
    internal class LeadRest : NoteRest
    {
        ScoreWriter writer;
        double restvalue;
        int restlength; // In 128th notes

        public LeadRest(ScoreWriter writer, string value)
        {
            this.writer = writer;
            restvalue = ReadNoteValue(value);
            restlength = CalculateLength();
        }

        public void Write()
        {
            XmlDocument creator = new XmlDocument();
            // We set the tempo back to normal, to more easily get the right amount of rests
            XmlElement settempo = creator.CreateElement("Tempo");
            XmlElement tempotag = creator.CreateElement("tempo");
            tempotag.InnerText = (writer.tempo / 60).ToString(System.Globalization.CultureInfo.InvariantCulture);
            settempo.AppendChild(tempotag);
            writer.AppendChild(settempo);

            int scribendum = restlength; // We will try to write rests as big as possible, for faster loading and easier reading
            XmlElement makerest = creator.CreateElement("Rest");
            XmlElement durationtag = creator.CreateElement("durationType");
            makerest.AppendChild(durationtag); // We can reuse the same Elements while writing
            while (scribendum > 0)
            {
                int log = (int)Math.Log2(scribendum); // Calculate required rest size
                string restname;
                switch (log)
                {
                    //case < 0:
                    case 0:
                        restname = "128th";
                        break;
                    case 1:
                        restname = "64th";
                        break;
                    case 2:
                        restname = "32nd";
                        break;
                    case 3:
                        restname = "16th";
                        break;
                    case 4:
                        restname = "eighth";
                        break;
                    case 5:
                        restname = "quarter";
                        break;
                    default: // Half rests are the biggest we add. Might start with whole rests later, but those are more complex because they involve measures
                        restname = "half";
                        log = 6;
                        break;
                }
                durationtag.InnerText = restname;
                writer.AppendChild(makerest);
                scribendum -= (int)Math.Pow(2, log);
            }
 
            writer.CountIncrease(restlength);
        }

        public int CalculateLength()
        {
            return (int)(restvalue * 128); // Smallest note unit we use is 128th
        }
    }
}

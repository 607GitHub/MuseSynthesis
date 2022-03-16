﻿using System;
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
            while (scribendum >= 64) // Might start with whole rests later, but those are more complex because they involve measures
            {
                durationtag.InnerText = "half";
                writer.AppendChild(makerest);
                scribendum -= 64;
            }
            while (scribendum >= 32)
            {
                durationtag.InnerText = "quarter";
                writer.AppendChild(makerest);
                scribendum -= 32;
            }
            while (scribendum >= 16)
            {
                durationtag.InnerText = "eigth";
                writer.AppendChild(makerest);
                scribendum -= 16;
            }
            while (scribendum >= 8)
            {
                durationtag.InnerText = "sixteenth";
                writer.AppendChild(makerest);
                scribendum -= 8;
            }

            writer.CountIncrease(restlength);
        }

        public int CalculateLength()
        {
            return (int)(restvalue * 128); // Smallest note unit we use is 128th
        }
    }
}

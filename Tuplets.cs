﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MuseSynthesis
{
    // Keeps track of an amount of tuples to be written. Can calculate portamento and create new Tupletss 
    internal class Tuplets
    {
        ScoreWriter writer;

        int length; // Amount of tuplets
        double starttempo; // When not doing a portamento, this will remain the tempo
        int drum; // What drum pitch to use
        int tupletdiv; // How many notes per 128th note
        string notevalue;

        // Portamento effect
        bool portamento;
        double goaltempo;
        double portfactor;
        bool outerportamento; // Whether this is the Tuplets spanning the entire leadnote; matters for the calculation

        public Tuplets(ScoreWriter writer, int length, double starttempo, int drum, int tupletdiv, string notevalue, bool portamento, double goaltempo, bool outerportamento)
        {
            this.writer = writer;
            this.length = length;
            this.starttempo = starttempo;
            this.drum = drum;
            this.tupletdiv = tupletdiv;
            this.notevalue = notevalue;
            this.portamento = portamento;
            this.goaltempo = goaltempo;
            this.outerportamento = outerportamento;

            portfactor = CalcPortfactor(); // Will be 1 if there is no portamento effect, and the factor that each tuplet should increase the tempo with otherwise
        }

        public void Write()
        {
            XmlDocument creator = new XmlDocument();

            // Write all tuplets
            for (int tuplet = 0; tuplet < length; tuplet++)
            {
                // For portamento we need to write a new tempo command for every tuplet
                XmlElement settempo = creator.CreateElement("Tempo");
                XmlElement tempotag = creator.CreateElement("tempo");
                double currenttempo = starttempo * Math.Pow(portfactor, tuplet);
                tempotag.InnerText = (currenttempo / 60).ToString(System.Globalization.CultureInfo.InvariantCulture);
                settempo.AppendChild(tempotag);
                writer.AppendChild(settempo);

                XmlElement maketuplet = creator.CreateElement("Tuplet");
                XmlElement normalnotestag = creator.CreateElement("normalNotes");
                normalnotestag.InnerText = tupletdiv.ToString();
                XmlElement actualnotestag = creator.CreateElement("actualNotes");
                actualnotestag.InnerText = tupletdiv.ToString();
                XmlElement basenotetag = creator.CreateElement("baseNote");
                basenotetag.InnerText = notevalue;
                maketuplet.AppendChild(normalnotestag);
                maketuplet.AppendChild(actualnotestag);
                maketuplet.AppendChild(basenotetag);
                writer.AppendChild(maketuplet);

                // Write all notes for a tuplet
                for (int note = 0; note < tupletdiv; note++)
                {
                    XmlElement makechord = creator.CreateElement("Chord"); // Not sure why the tag is called chord, but following it here for consistency
                    XmlElement durationtypetag = creator.CreateElement("durationType");
                    durationtypetag.InnerText = notevalue;
                    XmlElement makenote = creator.CreateElement("Note");
                    XmlElement pitchtag = creator.CreateElement("pitch");
                    pitchtag.InnerText = drum.ToString();
                    makenote.AppendChild(pitchtag);
                    makechord.AppendChild(durationtypetag);
                    makechord.AppendChild(makenote);
                    writer.AppendChild(makechord);
                }

                XmlElement endtuplet = creator.CreateElement("endTuplet");
                writer.AppendChild(endtuplet);
            }
        }

        public double CalcPortfactor()
        {
            if (!portamento)
                return 1;
            int steps = length;
            if (outerportamento) // If this is the outer portamento, we need to do the increase in one less step, because...
                steps--;         // at the first note we don't yet increase the tempo, and at the last note we do need to be at the end
            double tempoincrease = goaltempo / starttempo;
            portfactor = Math.Pow(tempoincrease, 1.0 / steps); // We need to multiply rather than add, because pitch is experienced logarithmically
            return portfactor;
        }
    }
}

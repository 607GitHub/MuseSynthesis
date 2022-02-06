﻿using System.Xml;
using System.Collections;

namespace MuseSynthesis
{
    // Reads the input XML and writes the output XML
    internal class ScoreWriter
    {
        private XmlDocument input; // Document to read from
        private XmlDocument output; // Document to write to
        public int tempo { get; private set; } // Current tempo
        public int a4tuning { get; private set; } // Frequency at which A4 should sound

        public ScoreWriter(XmlDocument output, XmlDocument input)
        {
            this.output = output;
            this.input = input;
            tempo = 120; // Default effective tempo; can be changed by command
            a4tuning = 440; // Tuning of A4 in Hertz; can be changed by command
        }

        // Write the score from the input
        public void WriteScore()
        {
            // We will read through all commands from top to bottom
            XmlNode scoreinput = input.SelectSingleNode("/museSynthesis/score");
            IEnumerator inputenumerator = scoreinput.GetEnumerator();
            while (inputenumerator.MoveNext())
            {
                XmlNode current = (XmlNode)inputenumerator.Current;
                switch (current.Name) // Take action depending on tag name
                {
                    case "tempo":
                        tempo = int.Parse(current.InnerText);
                        break;

                    case "tuning":
                        a4tuning = int.Parse(current.InnerText);
                        break;

                    case "leadnote":
                        string note = current.SelectSingleNode("note").InnerText;
                        string value = current.SelectSingleNode("value").InnerText;
                        LeadNote leadnote = new LeadNote(this, note, value);
                        leadnote.Write();
                        break;

                    default:
                        throw new ScoreWriterException("You used a non-supported node name: " + current.Name);
                }
            }
        }

        // Adds element to the output
        public void AppendChild(XmlElement appendum)
        {
            XmlNode child = output.ImportNode(appendum, true); // Nodes must be created in the right context, but we can import it 
            XmlNode outputnode = output.SelectSingleNode("/museScore/Score/Staff/Measure/voice"); // Navigate to the right place to write to
            outputnode.AppendChild(child);
        }
    }
}

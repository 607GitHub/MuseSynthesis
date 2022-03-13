﻿using System.Xml;
using System.Collections;
using System;

namespace MuseSynthesis
{
    // Reads the input XML and writes the output XML
    internal class ScoreWriter
    {
        private XmlDocument input; // Document to read from
        private XmlDocument output; // Document to write to
        public int tempo { get; private set; } // Current tempo
        public int a4tuning { get; private set; } // Frequency at which A4 should sound
        public int[] drums { get; private set; } // The drum sounds for each voice to use

        public bool displaytempos { get; private set; } // Whether to let MuseScore display the extra tempo commands; looks messy, but better for debugging

        public ScoreWriter(XmlDocument output, XmlDocument input)
        {
            this.output = output;
            this.input = input;
            tempo = 120; // Default effective tempo; can be changed by command
            a4tuning = 440; // Tuning of A4 in Hertz; can be changed by command
            drums = new int[1]; // Program might later manage multiple voices; for now the array will be one-dimensional
            drums[0] = 41; // Default drum; can be changed by command

            SetPreferences();
        }

        // Write the score from the input
        public void WriteScore()
        {
            // We will read through all commands from top to bottom
            XmlNode scoreinput = input.SelectSingleNode("/museSynthesis/score");
            IEnumerator inputenumerator = scoreinput.GetEnumerator();
            while (inputenumerator.MoveNext())
            {
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
                        string note = current.SelectSingleNode("note").InnerText;
                        string value = current.SelectSingleNode("value").InnerText;
                        XmlNode effects = current.SelectSingleNode("effects");
                        LeadNote leadnote = new LeadNote(this, note, value, effects);
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
    }
}

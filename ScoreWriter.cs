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
        public int[] velocities { get; private set; } // The velocities for each voice to use (from 1 to 127, default 64)
        private int songlength; // Keeps track of the total length of the notes and rests written, to decide on the time signature

        public bool displaytempos { get; private set; } // Whether to let MuseScore display the extra tempo commands; looks messy, but better for debugging

        public ScoreWriter(XmlDocument output, XmlDocument input)
        {
            this.output = output;
            this.input = input;
 
            tempo = 120; // Default effective tempo; can be changed by command
            a4tuning = 440; // Tuning of A4 in Hertz; can be changed by command
            SetupVoices();
            drums = new int[voices];
            drums[0] = 41; // Default drum; can be changed by command

            SetPreferences();
        }

        // Reads required amount of voices and sets up parts and staves for them
        private void SetupVoices()
        {
            XmlNode voices = input.SelectSingleNode("/museSynthesis/voices");
            if (voices == null)
                throw new ScoreWriterException("Please specify the amount of voices required.");
            this.voices = int.Parse(voices.InnerText);


            XmlNode outputscore = output.SelectSingleNode("/museScore/Score"); // Attach parts and staves to here

            // Adding parts
            for (int voice = 1; voice < this.voices; voice++) // First part is already in default.xml
            {
                XmlNode makepart = outputscore.SelectSingleNode("Part").Clone(); // Clone part node
                // Set correct voice id
                XmlElement makestaff = (XmlElement)makepart.SelectSingleNode("Staff"); // Same tag name, different place and purpose than makestaff below
                makestaff.SetAttribute("id", (voice + 1).ToString());
                outputscore.AppendChild(makepart); // Add new part
            }


            // Adding staves
            for (int voice = 0; voice < this.voices; voice++)
            {
                // Building up the required structure
                XmlElement makestaff = output.CreateElement("Staff");
                makestaff.SetAttribute("id", (voice + 1).ToString());
                XmlElement makemeasure = output.CreateElement("Measure");
                XmlElement makevoice = output.CreateElement("voice");
                XmlElement maketimesig = output.CreateElement("TimeSig");
                XmlElement signtag = output.CreateElement("sigN"); // Will be set by WriteScore
                XmlElement sigdtag = output.CreateElement("sigD");
                sigdtag.InnerText = "4";
                maketimesig.AppendChild(signtag);
                maketimesig.AppendChild(sigdtag);
                makevoice.AppendChild(maketimesig);
                makemeasure.AppendChild(makevoice);
                makestaff.AppendChild(makemeasure);

                outputscore.AppendChild(makestaff);
            }           
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
                        {   // Using code blocks allows reusing of variable names
                            int voice = int.Parse(current.GetAttribute("voice"));
                            drums[voice] = int.Parse(current.InnerText);
                            break;
                        }

                    case "velocity":
                        {   
                            int voice = int.Parse(current.GetAttribute("voice")); 
                            int velocity = int.Parse(current.InnerText);
                            XmlElement makedynamic = output.CreateElement("Dynamic");
                            XmlElement subtypetag = output.CreateElement("subtype");
                            subtypetag.InnerText = velocity.ToString(); // Would normally be p, mp, etc, but we might as well be exact
                            XmlElement velocitytag = output.CreateElement("velocity");
                            velocitytag.InnerText = velocity.ToString();
                            makedynamic.AppendChild(subtypetag);
                            makedynamic.AppendChild(velocitytag);
                            AppendChild(makedynamic, voice);
                        break;
                        }
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
                XmlNode timesig = output.SelectSingleNode("/museScore/Score/Staff[@id='"+(voice+1)+"']/Measure/voice/TimeSig");
                timesig.SelectSingleNode("sigN").InnerText = quarternotes.ToString();
            }
        }

        // Adds element to the specified voice in the output
        public void AppendChild(XmlElement appendum, int voice)
        {
            XmlNode child = output.ImportNode(appendum, true); // Nodes must be created in the right context, but we can import it 
            XmlNode outputnode = output.SelectSingleNode("/museScore/Score/Staff[@id='"+(voice+1)+"']/Measure/voice"); // Navigate to the right place to write to
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
            // Set creationDate
            XmlNode creationdate = output.SelectSingleNode("/museScore/Score/metaTag[@name='creationDate']");
            DateTime date = DateTime.Today;
            string datestring = date.ToString("yyyy-MM-dd");
            creationdate.InnerText = datestring;

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

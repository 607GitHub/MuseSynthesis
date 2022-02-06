// Goal of program: Make music for MuseScore by rapidly playing drums, generating the notes from user input. See https://youtu.be/2yoiIwVF88g for the idea behind it.

using System;
using System.Xml;
using System.IO;

namespace MuseSynthesis
{
    internal class Program
    {
        static int Main(string[] args)
        {
            
            // Read default XML
            XmlDocument output = new XmlDocument();
            output.Load("default.xml");

            // Read input XML
            XmlDocument input = new XmlDocument();
            string path = "example.xml";
            if (args.Length > 0)
                path = args[0];
            input.Load(path);

            UpdateMetaTags(output, input);

            ScoreWriter scorewriter = new ScoreWriter(output, input);
            scorewriter.WriteScore();

            // Write output XML
            string outputname; // Can be specified, will be the date otherwise
            string directory = ""; // Can be specified, otherwise the same as the input
            if (args.Length > 1)
            {
                outputname = args[1];
                if (Path.IsPathFullyQualified(outputname)) // If Drive letter is included, don't use the input path
                    directory = Path.GetDirectoryName(outputname);
                else
                    directory = Path.GetDirectoryName(path) + Path.GetDirectoryName(outputname);
                outputname = Path.GetFileName(outputname);
            }
            else
            {
                DateTime now = DateTime.Now;
                outputname = now.ToString("yyyy'-'MM'-'dd HH'.'mm'.'ss");
                directory = Path.GetDirectoryName(Path.GetFullPath(path)); // Default place to store is the same as where the input was
            }
            if (directory.Length > 0)
                Directory.CreateDirectory(directory);
            output.Save(directory + "/" + outputname + ".mscx");

            return 1;
        }

        // Update specified metaTags
        static void UpdateMetaTags(XmlDocument score, XmlDocument input)
        {
            XmlNodeList metatags = score.SelectNodes("/museScore/Score/metaTag");
            for (int tag = 0; tag < metatags.Count; tag++)
            {
                XmlElement metatag = (XmlElement)metatags[tag];
                string attribute = metatag.GetAttribute("name");
                XmlNode target = input.SelectSingleNode("/museSynthesis/metaTags/" + attribute);
                if (target != null)
                {
                    metatags[tag].InnerText = target.InnerText;
                }
            }
        }
    }
}
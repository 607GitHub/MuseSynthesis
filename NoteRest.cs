using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuseSynthesis
{   // Class that stores methods and properties that are shared between different classes for notes and rests
    internal class NoteRest
    {
        // Gets the note value from a string fraction
        protected double ReadNoteValue(string fraction)
        {
            string[] fractionparts = fraction.Split('/');
            int numerator, denominator;
            numerator = int.Parse(fractionparts[0]);
            if (fractionparts.Length > 1)
                denominator = int.Parse(fractionparts[1]);
            else
                denominator = 1; // For multiples of whole notes
            return (double)numerator / denominator;
        }
    }
}

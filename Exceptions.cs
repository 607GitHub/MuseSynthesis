using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuseSynthesis
{
    // I would like to later make these more custom, so that they will also communicate the line number. However, the way I am reading the XML document at the moment does not let me access the line number.
    public class ScoreWriterException : Exception
    {
        public ScoreWriterException(string message) : base(message) { }
    }

    public class LeadNoteException : Exception
    {
        public LeadNoteException(string message) : base(message) { }
    }
}

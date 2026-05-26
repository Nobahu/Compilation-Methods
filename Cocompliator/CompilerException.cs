using System;

namespace Cocompliator
{
    public class CompilerException : Exception
    {
        public int LineNumber { get; set; }
        public int CharPosition { get; set; }

        public CompilerException(string message, int lineNumber, int charPosition) : base(message)
        {
            LineNumber = lineNumber;
            CharPosition = charPosition;
        }
    }
}
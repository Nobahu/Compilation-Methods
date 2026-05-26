using System;

namespace Cocompliator
{
    public class Terminal
    {
        public TerminalType TerminalType { get; }
        public int CharPointer { get; set; }
        public int LinePointer { get; set; }

        public Terminal(TerminalType type, int linePointer, int charPointer)
        {
            TerminalType = type;
            LinePointer = linePointer;
            CharPointer = charPointer;
        }

        public class Number : Terminal
        {
            public int Data { get; }
            public Number(TerminalType type, int linePointer, int charPointer, string data) : base(type, linePointer, charPointer)
            {
                Data = Convert.ToInt32(data);
            }
        }

        public class Identifier : Terminal
        {
            public string Name { get; }
            public Identifier(TerminalType type, int linePointer, int charPointer, string data) : base(type, linePointer, charPointer)
            {
                Name = data;
            }
        }
    }
}
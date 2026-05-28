namespace Cocompliator
{
    public class RPNSymbol
    {
        public RPNType RPNType { get; set; }
        public int LinePointer { get; set; }
        public int CharPointer { get; set; }

        public RPNSymbol(RPNType type)
        {
            RPNType = type;
        }
    }

    public class RPNNumber : RPNSymbol
    {
        public int Data { get; set; }
        public double DoubleData { get; set; }
        public RPNNumber(RPNType type) : base(type) { }
    }

    public class RPNIdentifier : RPNSymbol
    {
        public string Name { get; set; }
        public RPNIdentifier(RPNType type) : base(type) { }
    }

    public class RPNMark : RPNSymbol
    {
        public MarkType MarkType { get; set; }
        public int? Position { get; set; }

        public RPNMark(RPNType type, MarkType markType) : base(type)
        {
            MarkType = markType;
        }
    }

    public class RPNTextLine : RPNSymbol
    {
        public string Data { get; set; }
        public RPNTextLine(RPNType type) : base(type) { }
    }

    public class RPNBoolean : RPNSymbol
    {
        public bool Data { get; set; }
        public RPNBoolean(RPNType type) : base(type) { }
    }

    public class RPNArrayAccess : RPNSymbol
    {
        public string ArrayName { get; set; }
        public int Index { get; set; }

        public RPNArrayAccess() : base(RPNType.F_Index) { }
    }
}
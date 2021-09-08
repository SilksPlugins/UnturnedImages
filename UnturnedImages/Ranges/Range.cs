namespace UnturnedImages.Ranges
{
    internal class Range
    {
        public ushort Start { get; }

        public ushort End { get; }

        public Range(ushort number) : this(number, number)
        {
        }

        public Range(ushort start, ushort end)
        {
            if (start <= end)
            {
                Start = start;
                End = end;
            }
            else
            {
                Start = end;
                End = start;
            }
        }

        public bool IsWithin(ushort number)
        {
            return number >= Start && number <= End;
        }
    }
}

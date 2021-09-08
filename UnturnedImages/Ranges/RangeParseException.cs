using System;

namespace UnturnedImages.Ranges
{
    internal class RangeParseException : Exception
    {
        public RangeParseException()
        {
        }

        public RangeParseException(string message) : base(message)
        {
        }

        public RangeParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

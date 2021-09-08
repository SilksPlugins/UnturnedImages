using System.Collections.Generic;
using System.Linq;

namespace UnturnedImages.Ranges
{
    internal class MultiRange
    {
        private readonly List<Range> _ranges;

        public IReadOnlyCollection<Range> Ranges => _ranges.AsReadOnly();

        public MultiRange(params Range[] ranges)
        {
            _ranges = ranges.ToList();
        }

        public bool IsWithin(ushort number)
        {
            return Ranges.Any(x => x.IsWithin(number));
        }
    }
}

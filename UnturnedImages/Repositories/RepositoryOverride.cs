using UnturnedImages.Ranges;

namespace UnturnedImages.Repositories
{
    internal class RepositoryOverride
    {
        public MultiRange Range { get; }

        public string Repository { get; }

        public RepositoryOverride(MultiRange range, string repository)
        {
            Range = range;
            Repository = repository;
        }
    }
}

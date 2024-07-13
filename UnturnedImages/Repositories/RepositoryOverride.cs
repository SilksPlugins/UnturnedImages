using Steamworks;
using System;
using UnturnedImages.Ranges;

namespace UnturnedImages.Repositories
{
    internal class RepositoryOverride
    {
        public MultiRange? Range { get; }
        public Guid? Guid { get; }
        public string? WorkshopId { get; } 
        public string Repository { get; }

        public RepositoryOverride(MultiRange range, string repository)
        {
            Range = range;
            Repository = repository;
        }

        public RepositoryOverride(Guid guid, string repository)
        {
            Guid = guid;
            Repository = repository;
        }

        public RepositoryOverride(string workshopId, string repository)
        {
            WorkshopId = workshopId;
            Repository = repository;
        }

        public bool Contains(Guid guid, ushort id, ulong workshopId)
        {
            return (Guid == guid) || (Range?.IsWithin(id) ?? false) || (WorkshopId?.Equals(workshopId) ?? false);
        }
    }
}

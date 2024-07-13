namespace UnturnedImages.Configuration.Overrides
{
    /// <summary>
    /// The config item which specifies repositories for certain asset IDs.
    /// </summary>
    public class OverrideConfig
    {
        /// <summary>
        /// The ID range to which this override applies to.
        /// </summary>
        public string? Id { get; set; } = string.Empty;

        /// <summary>
        /// The Guid to which this override applies to.
        /// </summary>
        public string? Guid { get; set; } = string.Empty;


        /// <summary>
        /// The workshopId to which this override applies to.
        /// </summary>
        public string? WorkshopId { get; set; } = string.Empty;

        /// <summary>
        /// The repository where the asset's image should be fetched.
        /// </summary>
        public string Repository { get; set; } = string.Empty;
    }
}

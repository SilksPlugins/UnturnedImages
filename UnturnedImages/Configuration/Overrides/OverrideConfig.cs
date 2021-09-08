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
        public string Id { get; set; } = "";

        /// <summary>
        /// The repository where the asset's image should be fetched.
        /// </summary>
        public string Repository { get; set; } = "";
    }
}

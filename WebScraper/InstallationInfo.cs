namespace NetEti.WebTools
{
    /// <summary>
    /// Contains the pathes to browser and driver.
    /// </summary>
    public class InstallationInfo
    {
        /// <summary>
        /// Contains the path to the subdirectory, where chrome was unzipped into.
        /// </summary>
        public string? RealBrowserPath { get; set; }

        /// <summary>
        /// Contains the path to the subdirectory, where chromedriver was unzipped into.
        /// </summary>
        public string? RealDriverPath { get; set; }
    }
}

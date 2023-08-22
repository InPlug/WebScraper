using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NetEti.WebTools
{
    /// <summary>
    /// Data-class to be filled(json) by an api-call to
    /// https://github.com/GoogleChromeLabs/chrome-for-testing#json-api-endpoints
    /// </summary>
    public class ChromeForTestingJsonApiDataContainer
    {
        /// <summary>
        /// Last actualization.
        /// </summary>
        public DateTime timestamp { get; set; }

        /// <summary>
        /// Four channels with different browsers and drivers (chrome).
        /// </summary>
        public Channels? channels { get; set; }

        /// <summary>
        /// Channel-class for channels with different browsers and drivers (chrome).
        /// </summary>
        public class Channels
        {
            /// <summary>
            /// The last stable build (recommendet).
            /// </summary>
            public Stable? Stable { get; set; }

            /// <summary>
            /// The Beta-build.
            /// </summary>
            public Beta? Beta { get; set; }

            /// <summary>
            /// The Dev-build.
            /// </summary>
            public Dev? Dev { get; set; }

            /// <summary>
            /// For future releases.
            /// </summary>
            public Canary? Canary { get; set; }
        }

        /// <summary>
        /// The Stable-channel.
        /// </summary>
        public class Stable
        {
            /// <summary>
            /// The name "Stable".
            /// </summary>
            public string? channel { get; set; }

            /// <summary>
            /// The version-number.
            /// </summary>
            public string? version { get; set; }

            /// <summary>
            /// The last revision-number.
            /// </summary>
            public string? revision { get; set; }

            /// <summary>
            /// List of urls to downloadable zip-archives of chrome-browsers and
            /// their corresponding chrome-drivers for different system-platforms.
            /// </summary>
            public Downloads? downloads { get; set; }
        }

        /// <summary>
        /// The Beta-channel.
        /// </summary>
        public class Beta
        {
            /// <summary>
            /// The name "Beta".
            /// </summary>
            public string? channel { get; set; }

            /// <summary>
            /// The version-number.
            /// </summary>
            public string? version { get; set; }

            /// <summary>
            /// The last revision-number.
            /// </summary>
            public string? revision { get; set; }

            /// <summary>
            /// List of urls to downloadable zip-archives of chrome-browsers and
            /// their corresponding chrome-drivers for different system-platforms.
            /// </summary>
            public Downloads? downloads { get; set; }
        }

        /// <summary>
        /// The Dev-channel.
        /// </summary>
        public class Dev
        {
            /// <summary>
            /// The name "Dev".
            /// </summary>
            public string? channel { get; set; }

            /// <summary>
            /// The version-number.
            /// </summary>
            public string? version { get; set; }

            /// <summary>
            /// The last revision-number.
            /// </summary>
            public string? revision { get; set; }

            /// <summary>
            /// List of urls to downloadable zip-archives of chrome-browsers and
            /// their corresponding chrome-drivers for different system-platforms.
            /// </summary>
            public Downloads? downloads { get; set; }
        }

        /// <summary>
        /// The Canary-channel.
        /// </summary>
        public class Canary
        {
            /// <summary>
            /// The name "Canary".
            /// </summary>
            public string? channel { get; set; }

            /// <summary>
            /// The version-number.
            /// </summary>
            public string? version { get; set; }

            /// <summary>
            /// The last revision-number.
            /// </summary>
            public string? revision { get; set; }

            /// <summary>
            /// List of urls to downloadable zip-archives of chrome-browsers and
            /// their corresponding chrome-drivers for different system-platforms.
            /// </summary>
            public Downloads? downloads { get; set; }
        }

        /// <summary>
        /// Contains list of urls to downloadable zip-archives of chrome-browsers and
        /// their corresponding chrome-drivers for different system-platforms.
        /// </summary>
        public class Downloads
        {
            /// <summary>
            /// List of urls to downloadable zip-archives of chrome-browsers
            /// for different system-platforms.
            /// </summary>
            public List<Chrome>? chrome { get; set; }

            /// <summary>
            /// List of urls to downloadable zip-archives of chrome-drivers
            /// for different system-platforms.
            /// </summary>
            public List<Chromedriver>? chromedriver { get; set; }

            /// <summary>
            /// List of urls to downloadable zip-archives of headless-shells 
            /// for chrome-browsers on different system-platforms.
            /// </summary>
            [JsonProperty("chrome-headless-shell")]
            public List<ChromeHeadlessShell>? chromeheadlessshell { get; set; }
        }

        /// <summary>
        /// Represents one url to a downloadable zip-archive containing an installation-directory
        /// with a standalone chrome-testbrowser for a specific system-platform.
        /// </summary>
        public class Chrome
        {
            /// <summary>
            /// The specific system-platform (linux64, mac-arm64, mac-x64, win32, win64).
            /// </summary>
            public string? platform { get; set; }

            /// <summary>
            /// Url to a downloadable zip-archive containing an installation-directory
            /// with a standalone chrome-testbrowser.
            /// </summary>
            public string? url { get; set; }
        }

        /// <summary>
        /// Represents one url to a downloadable zip-archive containing a chrome-driver
        /// corresponding to a standalone chrome-testbrowser for a specific system-platform.
        /// </summary>
        public class Chromedriver
        {
            /// <summary>
            /// The specific system-platform (linux64, mac-arm64, mac-x64, win32, win64).
            /// </summary>
            public string? platform { get; set; }

            /// <summary>
            /// Url to a downloadable zip-archive containing containing a chrome-driver
            /// corresponding to a standalone chrome-testbrowser.
            /// </summary>
            public string? url { get; set; }
        }

        /// <summary>
        /// Represents one url to a downloadable zip-archive containing a headless-shell
        /// corresponding to a standalone chrome-testbrowser for a specific system-platform.
        /// </summary>
        public class ChromeHeadlessShell
        {
            /// <summary>
            /// The specific system-platform (linux64, mac-arm64, mac-x64, win32, win64).
            /// </summary>
            public string? platform { get; set; }

            /// <summary>
            /// Url to a downloadable zip-archive containing a headless-shell
            /// corresponding to a standalone chrome-testbrowser.
            /// </summary>
            public string? url { get; set; }
        }

    }
}

namespace NetSpeedWidget.Resources
{
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    public class Strings
    {
        private static global::System.Resources.ResourceManager resourceMan;

        private static global::System.Globalization.CultureInfo resourceCulture;

        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        public static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("NetSpeedWidget.Resources.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        public static global::System.Globalization.CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Network Speed Widget.
        /// </summary>
        public static string AppTitle
        {
            get
            {
                return ResourceManager.GetString("AppTitle", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Error.
        /// </summary>
        public static string Error
        {
            get
            {
                return ResourceManager.GetString("Error", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Exit.
        /// </summary>
        public static string Exit
        {
            get
            {
                return ResourceManager.GetString("Exit", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Failed to apply language: {0}.
        /// </summary>
        public static string LanguageError
        {
            get
            {
                return ResourceManager.GetString("LanguageError", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Language Changed.
        /// </summary>
        public static string LanguageChanged
        {
            get
            {
                return ResourceManager.GetString("LanguageChanged", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Language.
        /// </summary>
        public static string Language
        {
            get
            {
                return ResourceManager.GetString("Language", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Failed to access registry.
        /// </summary>
        public static string RegistryAccessError
        {
            get
            {
                return ResourceManager.GetString("RegistryAccessError", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Language setting will be fully applied after restarting the application.
        /// </summary>
        public static string RestartRequired
        {
            get
            {
                return ResourceManager.GetString("RestartRequired", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Failed to save language setting: {0}.
        /// </summary>
        public static string SaveLanguageError
        {
            get
            {
                return ResourceManager.GetString("SaveLanguageError", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Settings.
        /// </summary>
        public static string Settings
        {
            get
            {
                return ResourceManager.GetString("Settings", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Show.
        /// </summary>
        public static string Show
        {
            get
            {
                return ResourceManager.GetString("Show", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Start with Windows.
        /// </summary>
        public static string StartWithWindows
        {
            get
            {
                return ResourceManager.GetString("StartWithWindows", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Failed to update startup settings: {0}.
        /// </summary>
        public static string StartupError
        {
            get
            {
                return ResourceManager.GetString("StartupError", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Network Usage.
        /// </summary>
        public static string NetworkUsage
        {
            get
            {
                return ResourceManager.GetString("NetworkUsage", resourceCulture);
            }
        }

        /// <summary>
        ///   Total Download: benzeri yerelleştirilmiş bir dize arar.
        /// </summary>
        public static string TotalDownload
        {
            get
            {
                return ResourceManager.GetString("TotalDownload", resourceCulture);
            }
        }

        /// <summary>
        ///   Total Upload: benzeri yerelleştirilmiş bir dize arar.
        /// </summary>
        public static string TotalUpload
        {
            get
            {
                return ResourceManager.GetString("TotalUpload", resourceCulture);
            }
        }

        /// <summary>
        ///   Download Speed (MB/s) benzeri yerelleştirilmiş bir dize arar.
        /// </summary>
        public static string DownloadSpeed
        {
            get
            {
                return ResourceManager.GetString("DownloadSpeed", resourceCulture);
            }
        }

        /// <summary>
        ///   Upload Speed (MB/s) benzeri yerelleştirilmiş bir dize arar.
        /// </summary>
        public static string UploadSpeed
        {
            get
            {
                return ResourceManager.GetString("UploadSpeed", resourceCulture);
            }
        }

        /// <summary>
        ///   Application Usage benzeri yerelleştirilmiş bir dize arar.
        /// </summary>
        public static string ApplicationUsage
        {
            get
            {
                return ResourceManager.GetString("ApplicationUsage", resourceCulture);
            }
        }
    }
}
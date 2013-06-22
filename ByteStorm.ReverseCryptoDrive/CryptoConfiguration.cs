using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ByteStorm.PassthroughDrive
{
    using System.Configuration;
    using System.IO;

    public class CryptoConfiguration
    {
        public static readonly string KEY_KEY = "Key";
        public static readonly string KEY_IV = "IV";
        public static readonly string KEY_DRIVELETTER = "DriveLetter";
        public static readonly string KEY_VOLUMELABEL = "VolumeLabel";
        public static readonly string KEY_MOUNTPATH = "MountPath";

        public static readonly string DEFAULT_DRIVELETTER = "R";

        private System.Configuration.Configuration config;

        public static readonly string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ByteStorm", "CryptoDrive", "config.xml");
        private static CryptoConfiguration defaultInstance = new CryptoConfiguration();

        public static CryptoConfiguration Instance
        {
            get
            {
                return defaultInstance;
            }
        }

        public CryptoConfiguration()
        {
            ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
            configMap.ExeConfigFilename = configPath;
            config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
        }

        public string getSetting(string key, string defaultValue)
        {
            KeyValueConfigurationElement elem = config.AppSettings.Settings[key];
            if (elem == null || elem.Value == null)
                return defaultValue;
            return elem.Value;
        }

        public void setSetting(string key, string value)
        {
            KeyValueConfigurationElement elem = config.AppSettings.Settings[key];
            if (elem == null)
                config.AppSettings.Settings.Add(key, value);
            else
                elem.Value = value;
        }

        public void save()
        {
            config.Save();
        }
    }
}

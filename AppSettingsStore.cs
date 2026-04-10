using System;
using System.IO;
using System.Xml.Serialization;

namespace KeyboardLayoutSwitcher
{
    public static class AppSettingsStore
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(AppSettings));

        public static AppSettings Load()
        {
            string settingsPath = GetSettingsPath();
            if (!File.Exists(settingsPath))
            {
                return new AppSettings();
            }

            try
            {
                using (FileStream stream = File.OpenRead(settingsPath))
                {
                    AppSettings settings = Serializer.Deserialize(stream) as AppSettings;
                    return settings ?? new AppSettings();
                }
            }
            catch
            {
                return new AppSettings();
            }
        }

        public static void Save(AppSettings settings)
        {
            string settingsPath = GetSettingsPath();
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));

            using (FileStream stream = File.Create(settingsPath))
            {
                Serializer.Serialize(stream, settings);
            }
        }

        private static string GetSettingsPath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appDataPath, "KeyboardLayoutSwitcher", "settings.xml");
        }
    }
}
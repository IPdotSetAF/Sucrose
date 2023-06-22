﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Sucrose.Manager
{
    public class SettingsManager
    {
        private readonly string _settingsFilePath;
        private readonly JsonSerializerSettings _serializerSettings;

        public SettingsManager(string settingsFileName, TypeNameHandling typeNameHandling = TypeNameHandling.None)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appName = "Sucrose";

            // Ayar dosyasının tam yolunu oluşturun
            _settingsFilePath = Path.Combine(appDataPath, appName, settingsFileName);

            // Ayar dosyasının dizinini oluşturun (varsa zaten var olmayacaktır)
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath));

            // JSON serileştirme ayarlarını yapılandırın
            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = typeNameHandling,
                Converters = new[]
                {
                    new IPAddressConverter()
                }
            };
        }

        public T GetSetting<T>(string key)
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = File.ReadAllText(_settingsFilePath);
                Settings? settings = JsonConvert.DeserializeObject<Settings>(json, _serializerSettings);
                if (settings.Properties.TryGetValue(key, out object value))
                {
                    return (T)value;
                }
            }

            return default;
        }

        public T GetSettingStable<T>(string key)
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = File.ReadAllText(_settingsFilePath);
                var settings = JsonConvert.DeserializeObject<Settings>(json);
                if (settings.Properties.TryGetValue(key, out object value))
                {
                    return JsonConvert.DeserializeObject<T>(value.ToString());
                }
            }

            return default;
        }

        public T GetSetting3<T>(string key)
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = File.ReadAllText(_settingsFilePath);
                var settings = JsonConvert.DeserializeObject<Settings>(json, _serializerSettings);
                if (settings.Properties.TryGetValue(key, out object value))
                {
                    return ConvertToType<T>(value);
                }
            }

            return default(T);
        }

        public void SetSetting<T>(string key, T value)
        {
            Settings settings;

            if (File.Exists(_settingsFilePath))
            {
                string json = File.ReadAllText(_settingsFilePath);
                settings = JsonConvert.DeserializeObject<Settings>(json, _serializerSettings);
            }
            else
            {
                settings = new Settings();
            }

            settings.Properties[key] = value;

            string serializedSettings = JsonConvert.SerializeObject(settings, _serializerSettings);
            File.WriteAllText(_settingsFilePath, serializedSettings);
        }

        private T ConvertToType<T>(object value)
        {
            if (typeof(T) == typeof(IPAddress))
            {
                return (T)(object)IPAddress.Parse(value.ToString());
            }

            return (T)value;
        }

        private class Settings
        {
            public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        }
    }
}
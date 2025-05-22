using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using NudgeTimer.Models;

namespace NudgeTimer.Services
{
    public class ConfigurationService
    {
        private readonly string _configPath;
        private TimerSettings _settings;

        public ConfigurationService()
        {
            _configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NudgeTimer",
                "config.json"
            );
            _settings = new TimerSettings();
        }

        public async Task<TimerSettings> LoadSettingsAsync()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = await File.ReadAllTextAsync(_configPath);
                    _settings = JsonSerializer.Deserialize<TimerSettings>(json) ?? new TimerSettings();
                }
            }
            catch (Exception)
            {
                // If there's any error, return default settings
                _settings = new TimerSettings();
            }
            return _settings;
        }

        public async Task SaveSettingsAsync(TimerSettings settings)
        {
            try
            {
                var directory = Path.GetDirectoryName(_configPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(_configPath, json);
                _settings = settings;
            }
            catch (Exception)
            {
                // Handle save error
            }
        }
    }
} 
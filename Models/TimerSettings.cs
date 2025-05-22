using System;
using System.Text.Json.Serialization;

namespace NudgeTimer.Models
{
    public class TimerSettings
    {
        [JsonPropertyName("targetMinutes")]
        public int TargetMinutes { get; set; } = 25;

        [JsonPropertyName("notificationType")]
        public NotificationType NotificationType { get; set; } = NotificationType.Notification;

        [JsonPropertyName("soundEnabled")]
        public bool SoundEnabled { get; set; } = true;

        [JsonPropertyName("taskbarFlashEnabled")]
        public bool TaskbarFlashEnabled { get; set; } = true;

        [JsonPropertyName("autoStart")]
        public bool AutoStart { get; set; } = false;

        [JsonPropertyName("runOnWindowsStartup")]
        public bool RunOnWindowsStartup { get; set; } = false;
    }

    [Flags]
    public enum NotificationType
    {
        None = 0,
        Notification = 1,
        Sound = 2,
        TaskbarFlash = 4,
        All = Notification | Sound | TaskbarFlash
    }
} 
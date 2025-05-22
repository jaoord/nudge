using System;
using System.Timers;
using System.Windows.Forms;
using NudgeTimer.Models;
using System.Media;
using System.IO;

namespace NudgeTimer.Services
{
    public class TimerService
    {
        private readonly System.Timers.Timer _timer;
        private DateTime _startTime;
        private TimeSpan _elapsedTime;
        private readonly NotifyIcon _notifyIcon;
        private TimerSettings _settings;
        private IntPtr _mainWindowHandle;
        private bool _isPaused = false;
        private TimeSpan _pausedElapsed = TimeSpan.Zero;

        public event EventHandler<TimeSpan>? TimeUpdated;
        public event EventHandler? TimerCompleted;

        public TimerService(NotifyIcon notifyIcon, TimerSettings settings)
        {
            _notifyIcon = notifyIcon;
            _settings = settings;
            _timer = new System.Timers.Timer(1000); // Update every second
            _timer.Elapsed += Timer_Elapsed;
            _elapsedTime = TimeSpan.Zero;
        }

        public void SetMainWindowHandle(IntPtr handle)
        {
            _mainWindowHandle = handle;
        }

        public void Start()
        {
            _startTime = DateTime.Now;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Reset()
        {
            _elapsedTime = TimeSpan.Zero;
            TimeUpdated?.Invoke(this, _elapsedTime);
        }

        public void UpdateSettings(TimerSettings settings)
        {
            _settings = settings;
        }

        public void Pause()
        {
            if (!_isPaused)
            {
                _timer.Stop();
                _pausedElapsed = DateTime.Now - _startTime;
                _isPaused = true;
            }
        }

        public void Resume()
        {
            if (_isPaused)
            {
                _startTime = DateTime.Now - _pausedElapsed;
                _timer.Start();
                _isPaused = false;
            }
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            _elapsedTime = DateTime.Now - _startTime;
            TimeUpdated?.Invoke(this, _elapsedTime);

            if (_elapsedTime.TotalMinutes >= _settings.TargetMinutes)
            {
                _timer.Stop();
                TriggerNotification();
                TimerCompleted?.Invoke(this, EventArgs.Empty);
                Reset();
                Start(); // Restart the timer
            }
        }

        private void TriggerNotification()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (_settings.NotificationType.HasFlag(NotificationType.Notification))
                {
                    _notifyIcon.ShowBalloonTip(
                        3000,
                        "Take a short break!",
                        $"{_settings.TargetMinutes} minutes have passed. Take a break.",
                        ToolTipIcon.Info
                    );
                }
                if (_settings.NotificationType.HasFlag(NotificationType.Sound) && _settings.SoundEnabled)
                {
                    SystemSounds.Asterisk.Play();
                }
                if (_settings.NotificationType.HasFlag(NotificationType.TaskbarFlash) && _settings.TaskbarFlashEnabled && _mainWindowHandle != IntPtr.Zero)
                {
                    TaskbarFlasher.Flash(_mainWindowHandle, 3);
                }
            });
        }

        private void CreateShortcut(string shortcutPath, string exePath)
        {
            // Use Windows Script Host to create a shortcut
            Type? wshShell = Type.GetTypeFromProgID("WScript.Shell");
            if (wshShell == null) return;

            dynamic? shell = Activator.CreateInstance(wshShell);
            if (shell == null) return;

            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            if (shortcut == null) return;

            shortcut.TargetPath = exePath;
            shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(exePath);
            shortcut.WindowStyle = 1;
            shortcut.Description = "Nudge Timer";
            shortcut.Save();
        }
    }

    // Helper class for taskbar flashing
    public static class TaskbarFlasher
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        public const uint FLASHW_STOP = 0;
        public const uint FLASHW_CAPTION = 1;
        public const uint FLASHW_TRAY = 2;
        public const uint FLASHW_ALL = 3;
        public const uint FLASHW_TIMER = 4;
        public const uint FLASHW_TIMERNOFG = 12;

        public static void Flash(IntPtr handle, uint count = 3)
        {
            FLASHWINFO fw = new FLASHWINFO
            {
                cbSize = Convert.ToUInt32(System.Runtime.InteropServices.Marshal.SizeOf(typeof(FLASHWINFO))),
                hwnd = handle,
                dwFlags = FLASHW_TRAY | FLASHW_TIMERNOFG,
                uCount = count,
                dwTimeout = 0
            };
            FlashWindowEx(ref fw);
        }
    }
} 
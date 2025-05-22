using System;
using System.Windows;
using NudgeTimer.Models;
using NudgeTimer.Services;
using System.Windows.Input;

namespace NudgeTimer
{
    public partial class SettingsWindow : Window
    {
        private readonly ConfigurationService _configService;
        private TimerSettings _settings;

        public SettingsWindow(ConfigurationService configService, TimerSettings currentSettings)
        {
            InitializeComponent();
            _configService = configService;
            _settings = currentSettings;
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            TargetMinutesTextBox.Text = _settings.TargetMinutes.ToString();
            ShowNotificationCheckBox.IsChecked = _settings.NotificationType.HasFlag(NotificationType.Notification);
            SoundEnabledCheckBox.IsChecked = _settings.SoundEnabled;
            TaskbarFlashCheckBox.IsChecked = _settings.TaskbarFlashEnabled;
            AutoStartCheckBox.IsChecked = _settings.AutoStart;
            RunOnWindowsStartupCheckBox.IsChecked = _settings.RunOnWindowsStartup;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate and update settings
                if (!int.TryParse(TargetMinutesTextBox.Text, out int targetMinutes) || targetMinutes <= 0)
                {
                    System.Windows.MessageBox.Show("Please enter a valid number of minutes.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _settings.TargetMinutes = targetMinutes;
                
                // Set notification type based on checkboxes
                _settings.NotificationType = NotificationType.None;
                if (ShowNotificationCheckBox.IsChecked == true)
                    _settings.NotificationType |= NotificationType.Notification;
                if (SoundEnabledCheckBox.IsChecked == true)
                    _settings.NotificationType |= NotificationType.Sound;
                if (TaskbarFlashCheckBox.IsChecked == true)
                    _settings.NotificationType |= NotificationType.TaskbarFlash;

                _settings.SoundEnabled = SoundEnabledCheckBox.IsChecked ?? false;
                _settings.TaskbarFlashEnabled = TaskbarFlashCheckBox.IsChecked ?? false;
                _settings.AutoStart = AutoStartCheckBox.IsChecked ?? false;
                _settings.RunOnWindowsStartup = RunOnWindowsStartupCheckBox.IsChecked ?? false;

                // Save settings
                await _configService.SaveSettingsAsync(_settings);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
} 
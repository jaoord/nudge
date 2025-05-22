using System;
using System.Windows;
using System.Windows.Forms;
using NudgeTimer.Services;
using NudgeTimer.Models;
using System.Windows.Input;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Windows.Resources;
using Microsoft.Win32;

namespace NudgeTimer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly NotifyIcon _notifyIcon;
    private readonly TimerService _timerService;
    private readonly ConfigurationService _configService;
    private TimerSettings _settings;

    private void ShowSettings()
    {
        var settingsWindow = new SettingsWindow(_configService, _settings);
        settingsWindow.Owner = this;
        
        if (settingsWindow.ShowDialog() == true)
        {
            // Settings were saved, reload them
            LoadSettingsAsync();
        }
    }

    public MainWindow()
    {
        InitializeComponent();

        // Initialize services
        _configService = new ConfigurationService();
        _settings = new TimerSettings(); // Initialize settings first

        // Load icon from embedded resource
        Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/nudge.ico")).Stream;
        Icon trayIcon = new Icon(iconStream);

        // Initialize notify icon
        _notifyIcon = new NotifyIcon
        {
            Icon = trayIcon,
            Visible = true,
            Text = "Nudge Timer - 00:00:00"
        };

        // Initialize timer service with initial settings
        _timerService = new TimerService(_notifyIcon, _settings);
        _timerService.TimeUpdated += TimerService_TimeUpdated;
        _timerService.TimerCompleted += TimerService_TimerCompleted;

        // Setup context menu
        var contextMenu = new System.Windows.Forms.ContextMenuStrip();
        contextMenu.Items.Add("Show", null, (s, e) => Show());
        contextMenu.Items.Add("Settings", null, (s, e) => ShowSettings());
        contextMenu.Items.Add("Exit", null, (s, e) => Close());
        _notifyIcon.ContextMenuStrip = contextMenu;

        // Handle window closing
        Closing += MainWindow_Closing;

        // Handle window state changes
        StateChanged += MainWindow_StateChanged;

        // Load settings after everything is initialized
        LoadSettingsAsync();

        // Set the window handle for taskbar flashing
        Loaded += (s, e) => _timerService.SetMainWindowHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);

        Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
    }

    private async void LoadSettingsAsync()
    {
        try
        {
            var loadedSettings = await _configService.LoadSettingsAsync();
            _settings = loadedSettings;
            _timerService.UpdateSettings(_settings);

            // Handle run on Windows startup
            HandleRunOnWindowsStartup(_settings.RunOnWindowsStartup);

            // Auto-start if enabled
            if (_settings.AutoStart)
            {
                StartButton_Click(this, new RoutedEventArgs());
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void HandleRunOnWindowsStartup(bool enable)
    {
        string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        string shortcutPath = System.IO.Path.Combine(startupFolder, "Nudge Timer.lnk");
        string exePath = Assembly.GetExecutingAssembly().Location;

        if (enable)
        {
            // Create shortcut
            CreateShortcut(shortcutPath, exePath);
        }
        else
        {
            // Remove shortcut if it exists
            if (System.IO.File.Exists(shortcutPath))
            {
                System.IO.File.Delete(shortcutPath);
            }
        }
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

    private void TimerService_TimeUpdated(object? sender, TimeSpan elapsedTime)
    {
        Dispatcher.Invoke(() =>
        {
            TimeDisplay.Text = elapsedTime.ToString(@"hh\:mm\:ss");
            _notifyIcon.Text = $"Nudge Timer - {elapsedTime:hh\\:mm\\:ss}";
        });
    }

    private void TimerService_TimerCompleted(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            // Timer will automatically restart
        });
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        _timerService.Start();
        StartButton.IsEnabled = false;
        StopButton.IsEnabled = true;
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        _timerService.Stop();
        _timerService.Reset();
        StartButton.IsEnabled = true;
        StopButton.IsEnabled = false;
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        ShowSettings();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        // Do not hide the window when minimized; keep it in the taskbar
        // if (WindowState == WindowState.Minimized)
        // {
        //     Hide();
        // }
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _notifyIcon.Dispose();
        Microsoft.Win32.SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
    }

    private void SystemEvents_SessionSwitch(object? sender, SessionSwitchEventArgs e)
    {
        if (e.Reason == SessionSwitchReason.SessionLock)
        {
            _timerService.Pause();
        }
        else if (e.Reason == SessionSwitchReason.SessionUnlock)
        {
            _timerService.Resume();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            this.DragMove();
    }
}
# Nudge Timer

A modern, beautiful, and highly configurable Windows timer app that lives in your taskbar and tray.  
Perfect for gentle reminders, focus sessions, or any recurring nudge you need!

---

## Features

- **Taskbar & Tray Integration:** Runs in the background, always accessible.
- **Configurable Timer:** Set your own countdown duration (in minutes).
- **Visual Digital Clock:** See the timer count up in a stylish, large display.
- **Multiple Notification Options:**
  - Windows notification
  - System sound
  - Taskbar icon flashing
- **Auto-Restart:** Timer restarts automatically after each cycle.
- **Auto-Start Option:** Start timer automatically when the app launches.
- **Modern UI:** Frameless, dark, rounded, and fully custom-drawn.
- **Settings Saved:** All preferences are stored in a JSON config file.

---

## Getting Started

### Prerequisites

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### Build & Run

```sh
git clone <your-repo-url>
cd NudgeTimer
dotnet build
dotnet run
```

### Packaging

To create a standalone `.exe`:

```sh
dotnet publish -c Release -r win-x64 --self-contained=true
```

---

## Customization

- No custom sound support. The app uses the default Windows system sound for notifications.

## License

This project is open source and available under the MIT License.

---

## Credits

- UI Design: Jogchem Andre Oord

---

> "Time you enjoy wasting is not wasted time." – Bertrand Russell 
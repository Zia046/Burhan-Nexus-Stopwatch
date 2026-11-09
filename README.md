# ⏱️ Burhan Nexus Stopwatch

**A Classic Digital Stopwatch for Your Windows Desktop**

Burhan Nexus Stopwatch is a lightweight, elegant, and modern Windows desktop application inspired by classic Casio digital watches. It brings a compact, always-on-top stopwatch experience directly to your desktop.

Designed for simplicity, accuracy, and everyday productivity.

---

## ✨ Features

- **Classic Digital Display:** Casio-inspired LCD-style stopwatch interface.
- **Live Clock & Date:** Displays current system time, date, and day.
- **Accurate Stopwatch:** High-precision time measurement.
- **Three Simple Controls:** Start, Pause, and Reset.
- **Resizable Window:** Freely resize the application using your mouse.
- **Always on Top:** Keep the stopwatch visible above other windows.
- **Compact Design:** Small desktop-friendly interface.
- **Customizable Settings:** Adjust clock format, display preferences, and themes.
- **Window Memory:** Remembers your preferred window position and size.
- **Offline Support:** Works without an internet connection.
- **Windows Compatibility:** Designed for Windows 10 and Windows 11.

---

## 🖥️ Interface

A miniature digital stopwatch inspired by the timeless design of classic Casio watches, featuring a clear digital display and simple controls.

---

## ⚙️ Technology

- C#
- WPF (Windows Presentation Foundation)
- .NET
- Native Windows Window Management
- High-Resolution Stopwatch Timing

---

## 🚀 Installation

1. Download the latest ZIP from the [Releases](../../releases) page.
2. Extract the application files if provided in a ZIP archive.
3. Run `BurhanNexusStopwatch.exe`.
4. Enjoy your stopwatch!

---

## 🛠️ Build from Source

**Requirements:** Windows 10/11 and the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or Visual Studio 2022 with the ".NET desktop development" workload).

Open `BurhanNexusStopwatch.sln` in Visual Studio and press **F5**, or from a terminal:

```powershell
# Run in debug mode
dotnet run --project src/BurhanNexusStopwatch

# Build the standalone Release exe (no .NET install needed on the target PC)
dotnet publish src/BurhanNexusStopwatch -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true -o publish
```

The finished app is `publish\BurhanNexusStopwatch.exe`.

Every push is also built automatically by GitHub Actions (see the **Actions** tab), and tags like `v1.0.0` publish a ready-to-download ZIP on the **Releases** page.

### Project layout

```
src/BurhanNexusStopwatch/
├── Controls/SevenSegmentDisplay.cs   # Casio-style LCD digits (vector, no font file)
├── Services/StopwatchEngine.cs       # Timing via System.Diagnostics.Stopwatch
├── Services/SettingsService.cs       # Settings saved to %AppData%\BurhanNexus\Stopwatch
├── Services/StartupService.cs        # "Start with Windows"
├── Services/NativeMethods.cs         # Native always-on-top (HWND_TOPMOST)
├── Themes/                           # Classic LCD, Dark, Light + shared styles
├── Views/                            # Settings and About dialogs
├── Models/AppInfo.cs                 # Branding and official contact details
└── MainWindow.xaml                   # Main stopwatch window
```

---

## 🎯 Main Controls

| Button   | Function                              |
|----------|---------------------------------------|
| START    | Start or resume stopwatch             |
| PAUSE    | Pause stopwatch                       |
| RESET    | Reset stopwatch to zero               |
| Settings | Customize application preferences     |
| About    | View software and developer information |

---

## 🏢 Developer

**Developed by Burhan Nexus**

Our mission is to create smart, reliable, and user-friendly software solutions that simplify everyday tasks.

- **Software:** Burhan Nexus Stopwatch
- **Version:** 1.0.0
- **Platform:** Windows 10 / Windows 11

---

## 📩 Contact

- **Brand:** Burhan Nexus
- **Mobile:** +923475093446
- **Email:** ziaullah.046@gmail.com

---

## 📄 License

Copyright © 2026 Burhan Nexus. All rights reserved.

Please refer to the repository's LICENSE file for usage and distribution permissions.

---

<p align="center">
Made with ❤️ by <b>Burhan Nexus</b><br>
<i>Innovation • Simplicity • Reliability</i>
</p>

# ZMK Split Battery Status

[![.NET Desktop Build](https://github.com/Maksim-Isakau/zmk-split-battery/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/Maksim-Isakau/zmk-split-battery/actions/workflows/dotnet-desktop.yml)

This app shows bluetooth device battery status in tray area.

It was designed to support devices with two or more batteries which statuses are reported via BLE GATT Characteristics.

Once one of the batteries level falls below 20% a toast notification is shown.

Dark/Light taskbar themes supported.

Requirements:
- Windows 10.0.19041 and above.
- .NET Desktop Runtime 9.0

## Building

This project uses .NET 9.0 and can be built using:

```bash
dotnet restore
dotnet build --configuration Release
```

Or using MSBuild:
```bash
msbuild ZMKSplit.sln /p:Configuration=Release /p:Platform="Any CPU"
```

### GitHub Actions

The project includes a GitHub Actions workflow that automatically builds the application on every push to `master`

## Screenshots:

![Screenshots](Screenshots/app-preview-top.png)
![Screenshots](Screenshots/app-preview-bottom-light.png)
![Screenshots](Screenshots/app-preview-bottom-dark.png)
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

The project includes a GitHub Actions workflow that automatically builds the application on every push to any branch and creates releases when you push version tags.

**To create a release:**
1. Create and push a version tag (e.g., `v1.0`):
   ```bash
   git tag v1.0
   git push origin v1.0
   ```
2. The workflow will automatically:
   - Build the Release configuration
   - Create a ZIP file with the compiled application
   - Create a GitHub release with the ZIP file attached
   - Generate release notes from recent commits

The workflow:
- Builds both on feature branches (for testing) and on version tags (for releases)
- Uses .NET 9.0 SDK and MSBuild for Windows-specific dependencies
- Creates downloadable artifacts for every build
- Runs on Windows runners to ensure compatibility with Windows Runtime APIs

## Screenshots:

![Screenshots](Screenshots/app-preview-top.png)
![Screenshots](Screenshots/app-preview-bottom-light.png)
![Screenshots](Screenshots/app-preview-bottom-dark.png)

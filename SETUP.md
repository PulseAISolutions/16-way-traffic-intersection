# Setup Instructions

## Prerequisites

Before you can build and run this project, you need to install:

### Option 1: Visual Studio 2022 (Recommended)
1. Download Visual Studio 2022 from https://visualstudio.microsoft.com/vs/
2. During installation, select:
   - ✅ Desktop & Mobile → "Desktop development with C++"
   - ✅ .NET desktop development workload
3. Launch Visual Studio and open `TrafficIntersection.csproj`
4. Visual Studio will automatically restore and build the project

### Option 2: .NET SDK + VS Code
1. Download .NET 6.0 or later SDK from https://dotnet.microsoft.com/download
2. Install it and restart your terminal
3. In PowerShell, navigate to the project and run:
   ```powershell
   dotnet restore
   dotnet build
   dotnet run
   ```

## Running the Application

### With Visual Studio 2022:
1. Open `TrafficIntersection.csproj` in Visual Studio
2. Press `F5` to start debugging
3. The application window will launch

### With Command Line:
```powershell
cd "d:\School\Traffic 4 way Intersection"
dotnet restore
dotnet build
dotnet run
```

## Troubleshooting

**Error: "No .NET SDKs were found"**
- Download and install .NET SDK from https://dotnet.microsoft.com/download

**Error: "The application 'build' does not exist"**
- Make sure you're using `dotnet build` not just `build`

**WPF window doesn't appear**
- Ensure you're running on Windows
- Check that you're using .NET 6.0-windows or later

## Project Files Created

- `TrafficIntersection.csproj` - Project configuration
- `App.xaml / App.xaml.cs` - Application entry point
- `MainWindow.xaml / MainWindow.xaml.cs` - Main UI window
- `Models/TrafficLight.cs` - Traffic light model
- `Models/Vehicle.cs` - Vehicle model
- `Logic/Intersection.cs` - Core simulation logic
- `README.md` - Project documentation
- `SETUP.md` - This file

# M4A to MP3 Converter
 
A Windows desktop app for converting `.m4a` audio files to `.mp3`. Add single files or whole folders, pick a bitrate, and convert in one batch.
 
## Features
 
- Add individual `.m4a` files or an entire folder
- Include or skip subfolders when adding a folder
- Choose a bitrate: 128k, 192k, 256k, or 320k (default: 320k)
- Save MP3s next to the source files, or to a chosen output folder
- Per-file status and error messages
- Progress bar for batch conversions
## Requirements
 
- Windows 10 or later
- [ffmpeg](https://ffmpeg.org/) available on your `PATH`
  - `winget install ffmpeg`
- To build from source: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
The app checks for ffmpeg at startup and shows install instructions if it is missing.
 
## Usage
 
1. Click **Add Files...** or **Add Folder...** to build the list.
2. Optionally choose an **Output folder** and a **Bitrate**.
3. Click **Convert**.
To remove entries, select them and click **Remove Selected**, or click **Clear** to empty the list. The list cannot be edited while a conversion is running.
 
## Build and run from source
 
```powershell
dotnet run
```
 
Or open `M4A2MP3.csproj` in Visual Studio 2022 and press F5.
 
## Publish a standalone build
 
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```
 
The executable is written to `publish\M4A2MP3.exe`. It bundles the .NET runtime, so users don't need .NET installed.
 
## Create an installer
 
The installer is built with [Inno Setup 6](https://jrsoftware.org/isinfo.php) using the `M4A2MP3.iss` script in the project root. After publishing, open the script and choose **Build > Compile**. The setup file is written to `installer\M4A2MP3-Setup-0.1.0.exe`.
 
## Project structure
 
```
M4A2MP3/
├── M4A2MP3.csproj        Project file (.NET 8, WPF)
├── App.xaml              Application entry point
├── MainWindow.xaml       UI layout
├── MainWindow.xaml.cs    UI logic and conversion queue
├── Models/
│   └── AudioFile.cs      File item with status and message
└── Services/
    └── Mp3Converter.cs   ffmpeg wrapper
```
 
## How it works
 
Each file is converted by running ffmpeg with the following arguments:
 
```
ffmpeg -nostdin -y -i <input.m4a> -vn -codec:a libmp3lame -b:a <bitrate> <output.mp3>
```
 
Files are converted one at a time. `-vn` drops any embedded cover art.
 
## Limitations
 
- Windows only (WPF).
- Cover art is not kept in the MP3.
- Output files are written to a single folder and do not preserve the source subfolder structure.
- Existing MP3 files with the same name are overwritten without a prompt.
## License
 
Add your license here.
 

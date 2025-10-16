# Radio Plugin for PowerToys Run

Search and play radio stations from PowerToys Run launcher.

![Radio Plugin](imagescreen.png)

## ✨ Features

- 🔍 **Smart Search**: Search stations by name, city, country, or genre
- 🌍 **30,000+ Stations**: Access worldwide radio stations via Radio Browser API
- 🎵 **Multi-Format**: Support for MP3, AAC, OGG, FLAC, and more
- 📊 **Rich Details**: View bitrate, codec, country, and tags
- ⚡ **Fast & Reliable**: Automatic retry with multiple API mirrors
- 🔄 **High Availability**: 5+ API servers for redundancy
- 🎨 **Theme Support**: Beautiful icons for light and dark themes
- 🔒 **Privacy First**: No tracking, no data collection, open source
- 📝 **Debug Logging**: Detailed logs for troubleshooting
- 🌐 **Global Coverage**: Stations from every continent

## 📥 Installation

### Method 1: Manual Installation (Recommended)

1. Download the latest release:
   - For 64-bit Windows: `Radio-1.0.0-x64.zip`
   - For ARM64 Windows: `Radio-1.0.0-arm64.zip`

2. Extract the ZIP file to PowerToys Run plugins directory:
   ```
   %LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\Radio\
   ```
   
   Full path example:
   ```
   C:\Users\YourUsername\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\Radio\
   ```

3. Restart PowerToys Run:
   - Close PowerToys Run (Alt+F4)
   - Open PowerToys settings
   - Toggle PowerToys Run off and on

### Method 2: PowerShell Installation

```powershell
# Create plugin directory
$pluginDir = "$env:LOCALAPPDATA\Microsoft\PowerToys\PowerToys Run\Plugins\Radio"
New-Item -ItemType Directory -Force -Path $pluginDir

# Download and extract (replace with actual download URL)
# Invoke-WebRequest -Uri "https://github.com/yourusername/Radio/releases/latest/download/Radio-1.0.0-x64.zip" -OutFile "$env:TEMP\Radio.zip"
# Expand-Archive -Path "$env:TEMP\Radio.zip" -DestinationPath $pluginDir -Force

# Restart PowerToys Run
Stop-Process -Name "PowerToys.PowerLauncher" -Force -ErrorAction SilentlyContinue
```

## 🚀 Usage

### Basic Search

Open PowerToys Run (Alt+Space) and type:

```
radio [search term]
```

### Search Examples

**By City:**
```
radio lviv
radio london  
radio paris
radio new york
```

**By Country:**
```
radio ukraine
radio germany
radio france
radio usa
```

**By Station Name:**
```
radio bbc
radio npr
radio europa
```

**By Genre (in station name):**
```
radio rock
radio jazz
radio classical
radio news
```

### Actions

- **Enter** - Play station (opens in default media player)
- **Ctrl+C** - Copy stream URL to clipboard
- **Right-click** - Show context menu with options

## 🎛️ Configuration

### PowerToys Run Settings

1. Open PowerToys Settings
2. Go to PowerToys Run
3. Find "Radio" plugin in the list
4. Configure:
   - **Enable/Disable**: Toggle plugin on/off
   - **Direct Activation**: Set custom activation command
   - **Action Keyword**: Change default keyword

### System Requirements

- **Windows 10/11** (version 22H2 or later)
- **PowerToys** (version 0.70.0 or later)
- **Internet Connection**: Required for searching stations
- **Media Player**: Any player that supports streaming (Windows Media Player, VLC, etc.)

## 🔧 Troubleshooting

### Plugin Not Showing Up

1. **Check installation path:**
   ```
   %LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\Radio\
   ```

2. **Verify all files are present:**
   - Community.PowerToys.Run.Plugin.Radio.dll
   - plugin.json
   - Images folder with icons
   - Other required DLLs

3. **Restart PowerToys Run**

### "All Mirrors Failed" Error

This means the plugin cannot connect to Radio Browser API servers.

**Quick fixes:**
1. Check your internet connection
2. Disable VPN temporarily
3. Check Windows Firewall settings
4. Try again in a few minutes (servers might be temporarily down)

**Detailed troubleshooting:**
- See [TROUBLESHOOTING.md](TROUBLESHOOTING.md) for complete guide
- Check logs: `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\Radio\log.txt`

### Stations Not Playing

1. **Install a media player:**
   - Windows Media Player (built-in)
   - VLC Media Player
   - Any player that supports streaming

2. **Try different stations**: Some stations might be offline

3. **Copy URL and test in browser**: Right-click station → Copy URL → Paste in browser

### No Search Results

1. **Try different search terms:**
   - Use English names: "london" instead of "Londres"
   - Try country names: "ukraine", "germany"
   - Be more specific or more general

2. **Check spelling**

3. **Try station names**: "bbc", "npr", "radio france"

## 📝 Logs

Log file location:
```
%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\Radio\log.txt
```

The plugin logs:
- Search queries
- API calls and responses
- Errors and warnings
- Server selection

Use logs to diagnose issues or report bugs.

## 🛠️ Development

### Building from Source

Requirements:
- .NET 9.0 SDK
- PowerToys source code (for references)

Build command:
```bash
./build-and-zip.sh
```

Output:
- `Radio-1.0.0-x64.zip`
- `Radio-1.0.0-arm64.zip`

### Project Structure

```
Radio/
├── Community.PowerToys.Run.Plugin.Radio/
│   ├── Main.cs                     # Plugin entry point
│   ├── Core/
│   │   ├── Models/                 # Data models
│   │   └── Services/               # API clients
│   ├── Logging/                    # Logging infrastructure
│   └── Images/                     # Plugin icons
├── build-and-zip.sh                # Build script
└── plugin.json                     # Plugin metadata
```

### API Information

This plugin uses [Radio Browser API](https://www.radio-browser.info/):
- Free and open source
- Community-maintained database
- 30,000+ radio stations
- Regular updates
- Multiple API mirrors worldwide

## 📄 License

MIT License - see LICENSE file for details

## 🙏 Credits

- **Radio Browser API**: https://www.radio-browser.info/
- **PowerToys**: Microsoft PowerToys team
- **Community Templates**: Henrik Lau Eriksson

## 🐛 Bug Reports

Found a bug? Please create an issue with:
- Description of the problem
- Steps to reproduce
- Your Windows version
- PowerToys version
- Log file content (if applicable)

## 💡 Feature Requests

Have an idea? Create an issue with:
- Feature description
- Use case
- Expected behavior

## 🤝 Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## 📊 Statistics

- **Stations**: 30,000+
- **Countries**: 200+
- **Genres**: 500+
- **Languages**: 100+
- **API Uptime**: 99.9%+

## 🌟 Tips

1. **Favorite Stations**: Note down station names for quick access
2. **Local Stations**: Search by your city name
3. **Genre Exploration**: Try different genre names
4. **International**: Discover stations from other countries
5. **Quality**: Check bitrate in subtitle (higher = better quality)

---

**Made with ❤️ for PowerToys Run community**

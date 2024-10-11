## Introduction
HintServiceMeow(HSM) is a framework that allows plugins to display text on a selected position on a player's screen.

## Installation
To install this plugin, please 
1. go to the release page and download the newest HintServiceMeow.dll. Then, paste it into the plugin folder.
2. If you are using PluginAPI (The default API), put Harmony.dll into the dependencies folder.
3. Restart your server.

### Documents
- [Features Introduction](Features.md)
- [Getting Started](GettingStarted.md)
- [Core Features](CoreFeatures.md)

### FAQ
1. Why doesn't the plugin works?
- Make sure that HintServiceMeow is correctly installed.
- Check if there's any plugin that conflicts with HintServiceMeow
- Check if there's any error occurs when activating plugins.
2. Why does hints overlaps with each others
- This might happen when multiple plugins put their hint on the same position. You can check the config file for each plugin to adjust the position of their UI. If you cannot adjust the position using config file, please contact the author of the plugins.
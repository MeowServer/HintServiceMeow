Designed for Meow Servers~
HintServiceMeow is a plugin based on the Exiled framework that allows plugins to display multiple hints on a player's screen simultaneously. It also gives you a simple way to fix your hint onto a specific position on the player's screen.

### Installation
To install this plugin, please go to the release page and download the newest HintServiceMeow.dll. Then, paste it into the plugin folder of the Exiled framework. Restart your server and ensure you read the following information:
- This plugin is not compatible with any other plugins that use hints. It will also block all the server's functions related to hints (such as tips for picking up items).
- This plugin is still under development; please contact 2679977872@qq.com for any bugs.
- This plugin is designed for my own server. It should be compatible with any Exiled servers, but if not, please contact me.

### To Developers
Please use ReuI instead. ReuI is a much more mature framework.
If you insist on using this plugin instead of ReuI, here's an easy documentary about this plugin and a list of bugs I cannot solve myself:
1. First, there are 2 ways to show a hint to a player
- Create an instance of hint and add them to the PlayerDisplay instance corresponding to a player.
- Get PlayerUI corresponding to the player, and use the "ShowCommonHint" methods of that PlayerUI.
2. Showing hints by creating instances of hint and add them to the PlayerDisplay
  ```csharp
  var hint = Hint(100, HintAlignment.Left , "HelloWorld!") ;
  var playerDisplay = PlayerDisplay.Get(Player);
  playerDisplay.AddHint(hint);
  ```

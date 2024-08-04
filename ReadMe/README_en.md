## Introduction
HintServiceMeow is a plugin based on the Exiled framework that allows plugins to display multiple hints on a player's screen simultaneously. It also gives you a simple way to fix your hint onto a specific position on the player's screen.

## Installation
To install this plugin, please go to the release page and download the newest HintServiceMeow.dll. Then, paste it into the plugin folder of the Exiled framework. Restart your server and ensure you read the following information:
- This plugin is not compatible with any other plugins that use hints. It will also block all the server's functions related to hints (such as tips for picking up items).
- This plugin is still under development; please contact 2679977872@qq.com for any bugs.
- This plugin is designed for my own server. It should be compatible with any Exiled servers, but if not, please contact me.

## To Developers
Here's an easy documentary of this plugin:
1. First, there are 2 ways to show a hint to a player
- Create an instance of hint and add them to the PlayerDisplay instance corresponding to the player.
- Get PlayerUI corresponding to the player, and use the "ShowCommonHint" methods of that PlayerUI.
2. Showing hints by creating instances of hints and adding them to the PlayerDisplay
```csharp
var hint = new Hint() 
{
    AutoText = GetCurrentTime,
    YCoordinateAlign = HintVerticalAlign.Top,
    YCoordinate = 0,
    Alignment = HintAlignment.Right,
    FontSize = 20
}; //You do not have to set every single property
var playerDisplay = player.GetPlayerDisplay();
playerDisplay.AddHint(hint);
 ```
PlayerDisplay represents a player's screen. This is an example of creating a line of hint and adding it to the player's screen. Any change to the hint instance will be updated on the player's screen. Therefore, after adding the hint to playerDisplay, you can edit the content displayed on the screen by directly editing the content of the hint instance. For example, if I want to change the content to "你好，世界," I can do it by the following code.
```csharp
hint.Text = "你好，世界";
```
Y-coordinate: This represents the height of the hint. The higher the y-coordinate, the lower the hint is on the screen. The maximum y-coordinate you can use for a hint is 920. By experiment, the maximum y-coordinate you will need to put a hint on the bottom of the screen is 720.
HintAlignment: Choose whether to put the hint on the left, right, or middle of the player's screen.
Message: The content that will be displayed on the player's screen.
3. Showing hints by using PlayerUI
PlayerUI is an easier way to show hints to a player. It contains common hints, UI, and player effects (not finished). Common hints represent the hints that are commonly used in the plugins. This includes map hints, role hints, and item hints. This is an example of how to show a common hint to a player.
```csharp
var playerUI = player.GetPlayerUi();
playerUI.CommonHint.ShowOtherHint("HelloWorld!");
playerUI.CommonHint.ShowMapHint("RoomA", "This is room A");
playerUI.CommonHint.ShowRoleHint("CustomRole", description.split("\n"));
playerUI.CommonHint.ShowItemHint("CustomItem", "This is a custom item");
```

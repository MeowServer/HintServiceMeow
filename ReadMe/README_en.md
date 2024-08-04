## Introduction
HintServiceMeow is a plugin based on the Exiled framework that allows plugins to display multiple hints on a player's screen simultaneously. It also gives you a simple way to fix your hint onto a specific position on the player's screen.

## Installation
To install this plugin, please 
1. go to the release page and download the newest HintServiceMeow.dll. Then, paste it into the plugin folder. 
2. Restart your server.

## To Developers
Here's an easy documentary of this plugin:
There are 2 ways to show a hint to a player.
- Initate and Hint instance and add it to PlayerDisplay
- Use common hint component of PlayerUI
1. Showing hint using PlayerDisplay
```csharp
var hint = new Hint() 
{
    Text = "Hello World!"
    YCoordinateAlign = HintVerticalAlign.Top,//Means that Y coordinate represent the top side of the hint
    YCoordinate = 0, //Higher the Y coordinate, lower it is on the screen
    Alignment = HintAlignment.Right,
    FontSize = 20
}; //You do not have to set every single property
var playerDisplay = PlayerDisplay.Get(player);//Could be ReferenceHub or Player
playerDisplay.AddHint(hint);
 ```
PlayerDisplay represents a player's screen. This is an example of creating a hint and adding it to a player display. Any change to the hint instance will be automatically updated onto the player's screen. Therefore, after adding the hint to playerDisplay, you can edit the content displayed on the screen directly by editing the content of the hint instance. For example, if I want to change the content to "你好，世界," I can do it with the following code.
```csharp
hint.Text = "你好，世界";
```
2. Showing hints using PlayerUI
PlayerUI is an easier way to show hints to a player. The common hint component in PlayerUI cana help you show a commonly used hint to a player. This includes map hints, role hints, and item hints. Here is an example of how to show a common hint to a player.
```csharp
var playerUi = PlayerUI.Get(player);//Could be ReferenceHub or Player
playerUi.CommonHint.ShowOtherHint("HelloWorld!");
playerUi.CommonHint.ShowMapHint("RoomA", "This is room A");
playerUi.CommonHint.ShowRoleHint("CustomRole", description.split("\n"));
playerUi.CommonHint.ShowItemHint("CustomItem", "This is a custom item");
```

## Introduction
HintServiceMeow is a framework that allows plugins to display multiple hints on a player's screen simultaneously.

## Installation
To install this plugin, please 
1. go to the release page and download the newest HintServiceMeow.dll. Then, paste it into the plugin folder.
2. If you are using PluginAPI (The default API), put Harmony.dll into dependencies folder.
3. Restart your server.

## Features
HintServiceMeow includes following features:
#### Hint Adapter
HintServiceMeow includes a hint adapter that can automatically convert other plugins' hints into HintServiceMeow-compatible Hint. This allows people to use any plugin that uses hints, even if they are neither compatible with each other nor with HintServiceMeow. 

## Developer Features
#### Auto text
An auto-update text in the Hint classes.
#### Dynamic Hint
A hint that can automatically position itself on the screen in the most optimal position. 
#### Auto Update
Any changes to a hint will be automatically updated on the player's screen.
#### Update forecast
This framework can analyze the update frequency of each hint and schedule updates accordingly. You can also customize the update delay for each Hint, ranging from Fastest to No-sync.
#### Player UI
Player UI includes advanced components to simplify your development. For example, the common hint component of PlayerUI allows you to show commonly used hints to a player using a single method.

## To Developers
Here's an easy documentary of this plugin:
There are 2 ways to show a hint to a player.
- Initate and Hint instance and add it to PlayerDisplay
- Use common hint component of PlayerUI
1. Showing hint using PlayerDisplay
```csharp
var hint = new Hint() 
{
    Text = "Hello World!",
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
PlayerUI is an easier way to show hints to a player. The common hint component in PlayerUI includes several methods for you to show commonly used hints, such as map hints, role hints, and item hints. Here is an example of how to show a common hint to a player.
```csharp
var playerUi = PlayerUI.Get(player);//Could be ReferenceHub or Player
playerUi.CommonHint.ShowOtherHint("HelloWorld!");
playerUi.CommonHint.ShowMapHint("RoomA", "This is room A");
playerUi.CommonHint.ShowRoleHint("CustomRole", description.split("\n"));
playerUi.CommonHint.ShowItemHint("CustomItem", "This is a custom item");
```

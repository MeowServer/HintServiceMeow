为Meow服务器设计~
### 简介
HintServiceMeow 是一款基于Exiled框架的插件，用于向玩家同时展示多个Hint，并让每一条Hint固定在特定的位置。

### Installation
To install this plugin, please go to the release page and download the newest HintServiceMeow.dll. Then, paste it into the plugin folder of the Exiled framework. Restart your server and ensure you read the following information:
- This plugin is not compatible with any other plugins that use hints. It will also block all the server's functions related to hints (such as tips for picking up items).
- This plugin is still under development; please contact 2679977872@qq.com for any bugs.
- This plugin is designed for my own server. It should be compatible with any Exiled servers, but if not, please contact me.
希望安装这个插件，可以进入发行（Release）页面并下载最新的HintServiceMeow.dll文件。将这个文件粘贴于Exiled框架的插件文件夹中，并重启服务器即可安装。在安装之前，请务必阅读以下事项：
- 这个插件和其他任何使用了Hint的插件均不兼容，其会阻止所有的使用了Hint的服务器或插件的功能（比如背包物品提示等）
- 

### To Developers
Please use [RueI](https://github.com/Ruemena/RueI) instead. RueI is a much more mature framework.
If you insist on using this plugin instead of RueI, here's an easy documentary about this plugin:
1. First, there are 2 ways to show a hint to a player
- Create an instance of hint and add them to the PlayerDisplay instance corresponding to a player.
- Get PlayerUI corresponding to the player, and use the "ShowCommonHint" methods of that PlayerUI.
2. Showing hints by creating instances of hints and adding them to the PlayerDisplay
```csharp
var hint = new Hint(100, HintAlignment.Left , "HelloWorld!") ;
var playerDisplay = PlayerDisplay.Get(player);
playerDisplay.AddHint(hint);
 ```
This is an example of creating a line of hints and adding it to the player's screen. Any change to the instance of the hint will be updated directly on the player's screen. Therefore, after adding the hint onto playerDisplay, you can edit the hint's content by directly editing the content of the instance of the hint. For example, if I want to change the content of the hint to "你好，世界," I can do it by the following code.
```csharp
hint.message = "你好，世界";
```
Y-coordinate: represents the height of the hint. The higher the y-coordinate is, the lower the hint is on the screen. The maximum y-coordinate you can use for a hint is 920. By experiment, the maximum y coordinate you will need to put a hint on the bottom of the screen is 720.
HintAlignment: Whether to put the hint on the left, right, or the middle of the player's screen.
Message: The content of the hint.
3. Showing hints by using PlayerUI
```csharp
var playerUI = playerUI.Get(player);
playerUI.ShowOtherHint("HelloWorld!");
playerUI.ShowMapHint("RoomA", "This is room A");
playerUI.ShowRoleHint("CustomRole", description.split("\n"));
playerUI.ShowItemHint("CustomItem", "This is a custom item");
```
PlayerUI is an easier way to show hints to a player. It contains common hints, UI, and player effects (not finished). Common hints represent the hints that are commonly used in the game. This includes map hints, role hints, item hints, and others. This is an example of how to show a simple, commonly used hint to a player.
### Bugs
Here are some bugs that might appear while using this plugin:
- When players have reached the limit of the amount of ammo they can carry, they will receive a hint that tells them that they cannot pick more ammo. This hint will interrupt the hint service. To solve this problem, PlayerDisplay will update the hint every 5 seconds. However, the player will not be able to see any hints before the updation is finished.
- When too many hints are displayed in similar positions, the position of each hint might be less accurate.
### Update Plan
The next update will be the config for PlayerUI, allowing you to control and customize PlayerUI.

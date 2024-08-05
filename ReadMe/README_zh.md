### 简介
HintServiceMeow 是一款基于Exiled框架的插件，用于向玩家同时展示多条Hint。
### 安装
如果希望安装这个插件
1. 进入发行（Release）页面并下载最新的HintServiceMeow.dll文件。
2. 如果您正在使用PluginAPI（默认的API），请将Harmony.dll放入dependencies文件夹
3. 将这个文件粘贴于插件文件夹中，并重启服务器即可安装。

### 致开发者
这里是一个简单的指导：
有两种方式向一个玩家展示一条Hint
- 创建一个hint类型的实例，并将其加入到PlayerDisplay类中
- 使用PlayerUI的CommonHint部件
2. 通过PlayerDisplay来展示Hint
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
这个例子创建了一条Hint实例，并将其放置到到玩家的屏幕上。任何对Hint实例的改变都会自动更新到玩家屏幕上。因此，在添加了Hint之后，我可以通过改变Hint实例来修改玩家所看到的内容。比如，如果我希望玩家的屏幕上展示“你好，世界”而非“Hello World!"的话，我可以这么做：
```csharp
hint.Text = "你好，世界";
```
3. 通过PlayerUI展示Hint
PlayerUI相比PlayerDisplay更见的简单易用，其中的CommonHint部件包含了了插件经常用到的几种提示，这包含地图提示，角色提示，物品提示，和其他提示。以下是一个使用CommonHint来给予玩家提示的例子
```csharp
var playerUi = PlayerUI.Get(player);//Could be ReferenceHub or Player
playerUi.CommonHint.ShowOtherHint("HelloWorld!");
playerUi.CommonHint.ShowMapHint("RoomA", "This is room A");
playerUi.CommonHint.ShowRoleHint("CustomRole", description.split("\n"));
playerUi.CommonHint.ShowItemHint("CustomItem", "This is a custom item");
```


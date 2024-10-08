## 简介
HintServiceMeow 是一个的Hint框架，用于在玩家的屏幕上显示文字

## 安装
如果希望安装这个插件
1. 进入发行（Release）页面并下载最新的HintServiceMeow.dll文件。
2. 如果您正在使用PluginAPI（默认的API），请将Harmony.dll放入dependencies文件夹
3. 将这个文件粘贴于插件文件夹中，并重启服务器即可安装。

## 功能
#### Hint适配器
HintServiceMeow包含了一个Hint适配器，这个适配器可以自动将本不适配HintServiceMeow的插件适配HintServiceMeow。通过这样，可以让使用了Hint的插件在没有任何适配的情况下互相兼容。

## 开发者功能
#### 基本功能
这个框架提供了在不和其他插件冲突的情况下，在玩家屏幕的选定位置上显示文字的功能
#### 自更新文字
可通过AutoText属性自动更新Hint内容
#### 动态Hint
一种自动插入到屏幕最佳位置的Hint
#### 自动更新
任何对Hint类属性的改变都会自动更新到玩家的屏幕上
#### 更新预测
这个框架可以分析并预测每个Hint的更新速度，并根据更新速度规划更新时间。您可以通过改变Hint中的SyncSpeed属性来调整每个Hint的更新延时，可选择从最快到不同步。
#### 玩家UI
玩家UI包含了一系列的简化开发的部件。其中的CommonHint部件可以帮助开发者快速的向玩家展示常用的Hint。
 

## 致开发者
这里是一个简单的指导：
有两种方式向一个玩家显示一段文字
- 创建一个Hint类型的实例，并将其加入到PlayerDisplay类中
- 使用PlayerUI的CommonHint部件
2. 通过PlayerDisplay来显示文字
```csharp
var hint = new Hint() 
{
    Text = "Hello World!",
    YCoordinateAlign = HintVerticalAlign.Top,//这表示Y坐标代表了文字的上边缘，换句话说，文字的上边缘在y=0
    YCoordinate = 0, // Y坐标越高，文字的位置越低
    Alignment = HintAlignment.Right,
    FontSize = 20
}; //你不必设置每一个属性
var playerDisplay = PlayerDisplay.Get(player);//你可以使用ReferenceHub或者Player来获取PlayerDisplay
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


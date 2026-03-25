点击[此处](/Docs/SimplifiedChinese/README.md)返回 README

## 开始使用
### 设置依赖
1. 创建您的 C# 项目
2. 将从发行页面下载的 dll 文件添加到项目的依赖中
### 显示您的第一个 Hint
以下代码块展示了如何使用 HSM 的常用功能。

---
在玩家屏幕上创建一个 "Hello World" 提示。
```CSharp
Player player = Player.Get(xxx);

// Hint 是一个简单的提示，可以显示在玩家的屏幕上。
Hint hint1 = new Hint
{
    Text = "Hello World" // 您可以在一对大括号({})中设置提示的属性
};

// 您可以这样设置提示的属性：
hint1.FontSize = 40;
hint1.YCoordinate = 700;
hint1.Alignment = HintAlignment.Left;
// 设置属性后，您无需调用任何方法来请求更新。所有更新将由 HSM（HintServiceMeow）自动完成。

// 您可以通过将提示添加到玩家的 PlayerDisplay 来向玩家显示它。
// 也可以通过从 PlayerDisplay 中移除来删除它。
PlayerDisplay playerDisplay = PlayerDisplay.Get(player);
playerDisplay.AddHint(hint1);
// playerDisplay.RemoveHint(hint);

```
---
使用 `AutoText` 创建自动更新内容的提示。

使用扩展方法以更简便的方式添加或移除提示。
```CSharp
Hint hint2 = new Hint
{
    AutoText = ev => DateTime.Now.ToString("HH:mm:ss"), // 您也可以使用函数来设置提示的文字，提示将自动更新自身。
    Alignment = HintAlignment.Right, // 您可以按任意顺序设置提示的属性，也可以选择不设置某些属性，因为所有属性都有默认值。
    YCoordinate = 200
};

// 您也可以使用扩展方法使操作更简便
player.AddHint(hint2); // 这等同于 playerDisplay.AddHint(hint);
// player.RemoveHint(hint); // 这等同于 playerDisplay.RemoveHint(hint);

```
---
使用 `NextUpdateDelay` 设置 AutoText 的自定义更新频率。

使用 `PlayerDisplay::ShowHint(Hint, float)` 在特定时间内显示提示。
```CSharp
Hint hint3 = new Hint()
{
    YCoordinate = 300,
    Alignment = HintAlignment.Right,
    AutoText = ev =>
    {
        ev.NextUpdateDelay = TimeSpan.FromSeconds(2f); // 您可以在事件参数中设置下次更新延迟，提示将在延迟后自动更新。这在您希望按特定时间间隔更新提示时非常有用。

        return "TPS: " + Server.Tps.ToString("F2");
    },
};

// 如果您只想临时显示提示，可以使用 ShowHint
playerDisplay.ShowHint(hint3, 12f); // 显示提示 12 秒后隐藏它。

```
---
使用 DynamicHint 帮助避免冲突。
```CSharp
// DynamicHint 是一种可以自动定位以避免与其他提示重叠的提示。
DynamicHint dynamicHint = new DynamicHint
{
    Text = "Hello Dynamic Hint",
    TargetX = 100f,
};

playerDisplay.AddHint(dynamicHint);

```
---
使用 CommonHint 快速开发您的 UI。
```CSharp
// PlayerUI::CommonHint 是一组预设提示，帮助您轻松显示提示
PlayerUI ui = PlayerUI.Get(player);
ui.CommonHint.ShowRoleHint("SCP173", ["杀死所有人类", "使用你的技能"]);
ui.CommonHint.ShowMapHint("重型收容区", "大多数 SCP 生成的地方");
ui.CommonHint.ShowItemHint("钥匙卡", "用于开门");
ui.CommonHint.ShowOtherHint("服务器正在启动！");
```
---
使用 `ResolutionOption` 适配不同的屏幕分辨率。
```CSharp
// ResolutionOption.Offset 会根据玩家的屏幕分辨率自动将左/右对齐的提示推向屏幕边缘。
Hint resolutionHint = new Hint
{
    Text = "Adapted to screen edge",
    Alignment = HintAlignment.Left,
    YCoordinate = 400,
    ResolutionOption = ResolutionOption.Offset // 这是默认值。设置为 ResolutionOption.None 可禁用此功能。
};

playerDisplay.AddHint(resolutionHint);

```
---
使用 `RichTag` 辅助工具轻松应用 Unity 富文本标签。
```CSharp
// 使用 / 运算符或 UseTag() 扩展方法为文本包裹富文本标签
string colored = "Hello" / ColorTag.Red; // 结果: <color=#FF0000>Hello</color>
string bold = "World".UseTag(BoldTag.Bold); // 结果: <b>World</b>
string styled = "Fancy".UseTag(ColorTag.Get("#FF8800"), BoldTag.Bold, SizeTag.Get(30)); // 组合多个标签

Hint richHint = new Hint
{
    Text = colored + " " + bold + "\n" + styled,
    YCoordinate = 500,
};

playerDisplay.AddHint(richHint);

```
---
使用 `Transition` 为提示属性变化添加动画效果。
```CSharp
// Transition 会在指定时间内平滑地动画化属性变化
Hint animatedHint = new Hint
{
    Text = "I move smoothly!",
    YCoordinate = 600,
    FontSize = 20,
    YCoordinateTransition = Transition.Get(EasingType.EaseInOut, duration: 1f), // 在 1 秒内动画化 Y 位置变化
    FontSizeTransition = Transition.Get(EasingType.EaseOut, duration: 0.5f), // 在 0.5 秒内动画化字体大小变化
};

playerDisplay.AddHint(animatedHint);

// 当你更改属性时，过渡效果会平滑地进行动画
animatedHint.YCoordinate = 200; // 在 1 秒内从 600 平滑移动到 200
animatedHint.FontSize = 40; // 在 0.5 秒内从 20 平滑增长到 40

```
---
使用 `HintTemplate` 快速创建具有预设属性的提示。
```CSharp
// HintTemplate 是一个用于创建具有预定义属性的提示的蓝图
HintTemplate template = new HintTemplate
{
    FontSize = 25,
    YCoordinate = 700,
    Alignment = HintAlignment.Right,
    FontSizeTransition = Transition.Get(EasingType.EaseInOut, 0.5f),
};

// 使用 GetHint() 从模板创建新提示
Hint hintFromTemplate = template.GetHint();
hintFromTemplate.Text = "Created from template";
playerDisplay.AddHint(hintFromTemplate);

// 或使用 Apply() 将模板应用到现有提示
Hint existingHint = new Hint { Text = "Existing hint" };
template.Apply(existingHint); // 应用 FontSize、YCoordinate、Alignment 和 FontSizeTransition
playerDisplay.AddHint(existingHint);

```
---
上述代码块将创建如下所示的 UI：
![提示视图](Images/GettingStartedExample.jpg)
标注版：
![标注版提示视图](Images/GettingStartedExampleLabeled.jpg)

阅读[核心功能](CoreFeatures.md)了解更多

点击[此处](/Docs/SimplifiedChinese/README.md)返回 README

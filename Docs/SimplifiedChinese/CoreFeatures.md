点击[此处](/Docs/SimplifiedChinese/README.md)返回 README

# 核心功能

本文档涵盖了 HintServiceMeow 的公共 API，按功能模块组织。

---

## 目录

- [核心功能](#核心功能)
  - [目录](#目录)
  - [Hint 模型](#hint-模型)
    - [AbstractHint](#abstracthint)
    - [Hint](#hint)
    - [DynamicHint](#dynamichint)
  - [PlayerDisplay](#playerdisplay)
  - [UI 层](#ui-层)
    - [PlayerUI](#playerui)
    - [CommonHint](#commonhint)
  - [扩展方法](#扩展方法)
    - [AbstractHint 扩展](#abstracthint-扩展)
    - [PlayerDisplay 扩展](#playerdisplay-扩展)
    - [NW Player 扩展](#nw-player-扩展)
    - [EXILED Player 扩展](#exiled-player-扩展)
  - [Hint 内容](#hint-内容)
    - [AbstractHintContent](#abstracthintcontent)
    - [StringContent](#stringcontent)
    - [AutoContent](#autocontent)
  - [过渡动画](#过渡动画)
    - [Transition 类](#transition-类)
    - [EasingType 枚举](#easingtype-枚举)
    - [Hint 过渡属性](#hint-过渡属性)
  - [富文本标签助手](#富文本标签助手)
    - [RichTag 基类](#richtag-基类)
    - [具体标签](#具体标签)
    - [字符串扩展](#字符串扩展)
  - [分辨率适配](#分辨率适配)
  - [模板](#模板)
    - [AbstractHintTemplate](#abstracthinttemplate-1)
    - [HintConfig](#hintconfig)
    - [HintTemplate](#hinttemplate)
    - [DynamicHintConfig](#dynamichintconfig)
    - [DynamicHintTemplate](#dynamichinttemplate)
    - [仅位置配置](#仅位置配置)

---

## Hint 模型

### AbstractHint

> 命名空间：`HintServiceMeow.Core.Models.Hints`

所有 Hint 类型的基类。**所有 Hint（`Hint`、`DynamicHint` 等）都继承自 `AbstractHint`**，它提供了文本内容、字体大小、同步速度和可见性等公共属性。它实现了 `INotifyPropertyChanged`，使属性更改自动触发显示更新。

创建任何类型的 Hint 时，无论具体的 Hint 类型如何，以下属性始终可用。

**属性：**

| 属性 | 类型 | 描述 |
|------|------|------|
| Guid | `Guid`（只读） | 自动生成的唯一标识符 |
| Id | `string` | 用于查找的自定义字符串标识符。默认值：`""` |
| SyncSpeed | `HintSyncSpeed` | 更新优先级。默认值：`Normal`。可选值：`Fastest`（192）——尽快更新，可能延迟其他提示；`Fast`（160）——在更改时立即计划更新；`Normal`（128）——标准速度；`Slow`（96）——先等待其他提示；`Slowest`（64）——等待更长时间；`UnSync`（32）——无自动同步，当其他提示触发同步时仍会更新 |
| FontSize | `int` | 文本字体大小。默认值：`20` |
| LineHeight | `float` | 行间额外垂直间距 |
| Content | `AbstractHintContent` | 此提示的内容提供者。默认值：`StringContent("")` |
| Text | `string?` | 获取/设置静态文本的快捷方式。设置此属性会用新的 `StringContent` 替换 `Content` |
| AutoText | `AutoContent.TextUpdateHandler?` | 获取/设置动态文本委托的快捷方式。设置此属性会用新的 `AutoContent` 替换 `Content` |
| Hide | `bool` | 提示是否隐藏。默认值：`false` |

**使用示例：**

```csharp
// 属性自动同步到玩家屏幕
hint.Text = "更新后的文字";
hint.FontSize = 30;
// 无需额外的方法调用
```

---

### Hint

> 命名空间：`HintServiceMeow.Core.Models.Hints`

在屏幕特定坐标处显示的固定位置提示。继承自 [AbstractHint](#abstracthint)。

**属性（除 AbstractHint 之外）：**

| 属性 | 类型 | 描述 |
|------|------|------|
| YCoordinate | `float` | 垂直位置。值越大，文本在屏幕上显示越靠下。默认值：`700` |
| XCoordinate | `float` | 水平偏移量。值越大，文本越靠右。默认值：`0` |
| Alignment | `HintAlignment` | 文本对齐方式。可选值：`Left`、`Right`、`Center`。默认值：`Center` |
| YCoordinateAlign | `HintVerticalAlign` | Y 坐标与文本的对齐方式。可选值：`Top`——Y 是顶部边缘；`Middle`——Y 是垂直中心；`Bottom`——Y 是底部边缘。默认值：`Middle` |

![Y 坐标示例](Images/YCoordinateExample.jpg)

**使用示例：**

```csharp
Hint hint = new Hint
{
    Text = "Hello World",
    FontSize = 40,
    YCoordinate = 700,
    Alignment = HintAlignment.Left
};

PlayerDisplay playerDisplay = PlayerDisplay.Get(player);
playerDisplay.AddHint(hint);
```

由于 HSM 具有自动更新功能，任何属性的更改都会自动反映在玩家屏幕上，无需额外的方法调用。

```csharp
hint.Text = "一些新文字";
// 无需额外的方法调用
```

---

### DynamicHint

> 命名空间：`HintServiceMeow.Core.Models.Hints`

自动定位以避免与其他提示重叠的提示。继承自 [AbstractHint](#abstracthint)。

**属性（除 AbstractHint 之外）：**

| 属性 | 类型 | 描述 |
|------|------|------|
| TopBoundary | `float` | 放置的上边界。默认值：`0` |
| BottomBoundary | `float` | 放置的下边界。默认值：`1000` |
| LeftBoundary | `float` | 放置的左边界。默认值：`-1200` |
| RightBoundary | `float` | 放置的右边界。默认值：`1200` |
| TargetX | `float` | 首选水平位置。默认值：`0` |
| TargetY | `float` | 首选垂直位置。默认值：`700` |
| TopMargin | `float` | 排列时提示上方的额外空间。默认值：`5` |
| BottomMargin | `float` | 排列时提示下方的额外空间。默认值：`5` |
| LeftMargin | `float` | 排列时左侧的额外空间。默认值：`100` |
| RightMargin | `float` | 排列时右侧的额外空间。默认值：`100` |
| Priority | `HintPriority` | 排列优先级。优先级更高的提示先排列。可选值：`Highest`（192）、`High`（160）、`Medium`（128）、`Low`（96）、`Lowest`（64）。默认值：`Medium` |
| Strategy | `DynamicHintStrategy` | 无可用空间时的行为。可选值：`Hide`——隐藏提示；`StayInPosition`——保持在目标位置。默认值：`Hide` |

**使用示例：**

```csharp
var dynamicHint = new DynamicHint
{
    Text = "Hello Dynamic Hint"
};

PlayerDisplay playerDisplay = PlayerDisplay.Get(player);
playerDisplay.AddHint(dynamicHint);
```

---

## PlayerDisplay

> 命名空间：`HintServiceMeow.Core.Utilities`

管理玩家提示显示的核心类。每个玩家拥有一个 `PlayerDisplay` 实例。

**事件：**

| 事件 | 类型 | 描述 |
|------|------|------|
| UpdateAvailable | `UpdateAvailableEventHandler` | 每次显示准备好更新时触发 |

**委托：** `delegate void UpdateAvailableEventHandler(UpdateAvailableEventArg ev)`

**属性：**

| 属性 | 类型 | 描述 |
|------|------|------|
| ReferenceHub | `ReferenceHub?`（只读） | 此显示所属的玩家 |
| HintParser | `IHintParser` | 将提示转换为富文本的解析器。可替换 |
| CompatibilityAdaptor | `ICompatibilityAdaptor` | 与其他插件兼容的适配器。可替换 |

**静态方法：**

| 方法 | 参数 | 返回值 | 描述 |
|------|------|--------|------|
| Get | `ReferenceHub referenceHub` | `PlayerDisplay` | 获取或创建玩家的 PlayerDisplay |
| Get | `LabApi.Features.Wrappers.Player player` | `PlayerDisplay` | 获取或创建 PlayerDisplay（NW/LabApi） |
| Get | `Exiled.API.Features.Player player` | `PlayerDisplay` | 获取或创建 PlayerDisplay（仅 EXILED） |

**实例方法：**

| 方法 | 参数 | 返回值 | 描述 |
|------|------|--------|------|
| AddHint | `AbstractHint? hint` | `void` | 向显示添加一个提示 |
| AddHint | `IEnumerable<AbstractHint>? hints` | `void` | 添加多个提示 |
| AddHint | `params AbstractHint[]? hints` | `void` | 添加多个提示（params） |
| AddHint | `AbstractHint? hint, string groupName` | `void` | 将提示添加到特定组 |
| ShowHint | `AbstractHint hint, float duration = 7f, AfterShowAction afterShow = AfterShowAction.Remove` | `void` | 添加提示并在 `duration` 秒后自动移除/隐藏。`AfterShowAction` 可选值：`Remove`——移除提示；`Hide`——设置 `Hide = true` |
| ShowHint | `IEnumerable<AbstractHint> hints, float duration = 7f, AfterShowAction afterShow = AfterShowAction.Remove` | `void` | 显示多个提示并自动移除 |
| RemoveHint | `AbstractHint? hint` | `void` | 移除一个提示 |
| RemoveHint | `IEnumerable<AbstractHint>? hints` | `void` | 移除多个提示 |
| RemoveHint | `params AbstractHint[]? hints` | `void` | 移除多个提示（params） |
| RemoveHint | `AbstractHint? hint, string groupName` | `void` | 从特定组移除提示 |
| RemoveHint | `string id` | `void` | 移除所有匹配给定 Id 的提示 |
| RemoveHint | `Guid id` | `void` | 移除匹配给定 Guid 的提示 |
| ClearHint | — | `void` | 移除调用程序集拥有的所有提示 |
| GetHint | `string? id` | `AbstractHint?` | 返回匹配 Id 的第一个提示 |
| GetHint | `Guid guid` | `AbstractHint?` | 返回匹配 Guid 的第一个提示 |
| GetHints | `string id` | `IEnumerable<AbstractHint>` | 返回所有匹配 Id 的提示 |
| GetHints | — | `IEnumerable<AbstractHint>` | 返回调用程序集拥有的所有提示 |
| HasHint | `string id` | `bool` | 检查是否存在具有给定 Id 的提示 |
| HasHint | `Guid guid` | `bool` | 检查是否存在具有给定 Guid 的提示 |
| TryGetHint | `string id, out AbstractHint hint` | `bool` | 尝试获取匹配 Id 的第一个提示 |
| TryGetHint | `Guid guid, out AbstractHint hint` | `bool` | 尝试获取匹配 Guid 的第一个提示 |
| TryGetHints | `string? id, out IEnumerable<AbstractHint> hints` | `bool` | 尝试获取所有匹配 Id 的提示 |
| ForceUpdate | `bool useFastUpdate = false` | `void` | 强制刷新显示。用于处理 `HintSyncSpeed.UnSync` 时使用 |
| SetMinUpdateInterval | `TimeSpan interval` | `void` | 设置更新之间的最小间隔 |
| AddDisplayOutput | `IDisplayOutput output` | `void` | 添加自定义显示输出 |
| RemoveDisplayOutput | `IDisplayOutput output` | `void` | 移除显示输出 |
| RemoveDisplayOutput\<T\> | — | `void` | 移除所有类型为 `T` 的显示输出（其中 `T : IDisplayOutput`） |

**使用示例：**

```csharp
PlayerDisplay pd = PlayerDisplay.Get(player);

// 添加提示
var hint = new Hint { Text = "你好", YCoordinate = 500 };
pd.AddHint(hint);

// 显示临时提示 5 秒
pd.ShowHint(new Hint { Text = "临时提示！" }, duration: 5f);

// 查找并修改提示
if (pd.TryGetHint("my-hint-id", out var found))
{
    found.Text = "已更新";
}

// 强制更新 UnSync 提示
pd.ForceUpdate();
```

---

## UI 层

### PlayerUI

> 命名空间：`HintServiceMeow.UI.Utilities`

提供访问 [CommonHint](#commonhint) 的每玩家 UI 外观类。

**属性：**

| 属性 | 类型 | 描述 |
|------|------|------|
| ReferenceHub | `ReferenceHub`（只读） | 底层玩家引用 |
| PlayerDisplay | `PlayerDisplay`（只读） | 玩家的 PlayerDisplay 实例 |
| CommonHint | `CommonHint`（只读） | 通用提示组件 |

**静态方法：**

| 方法 | 参数 | 返回值 | 描述 |
|------|------|--------|------|
| Get | `ReferenceHub referenceHub` | `PlayerUI` | 获取或创建玩家的 PlayerUI |
| Get | `LabApi.Features.Wrappers.Player player` | `PlayerUI` | 获取或创建 PlayerUI（NW/LabApi） |
| Get | `Exiled.API.Features.Player player` | `PlayerUI` | 获取或创建 PlayerUI（仅 EXILED） |

---

### CommonHint

> 命名空间：`HintServiceMeow.UI.Utilities`

为常见用例提供预配置的提示布局：物品描述、地图信息、职业信息和通用消息。

所有显示持续时间均可通过插件配置进行设置。每个重载中的 `time` 参数单位为秒。

**方法——物品提示：**

| 方法 | 参数 | 描述 |
|------|------|------|
| ShowItemHint | `string itemName` | 仅显示物品名称（短暂持续） |
| ShowItemHint | `string itemName, float time` | 仅显示物品名称，自定义持续时间 |
| ShowItemHint | `string itemName, string description` | 显示物品名称和一行描述 |
| ShowItemHint | `string itemName, string description, float time` | 显示物品名称和一行描述，自定义持续时间 |
| ShowItemHint | `string itemName, string[] description` | 显示物品名称和多行描述 |
| ShowItemHint | `string itemName, string[] description, float time` | 显示物品名称和多行描述，自定义持续时间 |

**方法——地图提示：**

| 方法 | 参数 | 描述 |
|------|------|------|
| ShowMapHint | `string roomName` | 仅显示房间名称（短暂持续） |
| ShowMapHint | `string roomName, float time` | 仅显示房间名称，自定义持续时间 |
| ShowMapHint | `string roomName, string description` | 显示房间名称和一行描述 |
| ShowMapHint | `string roomName, string description, float time` | 显示房间名称和一行描述，自定义持续时间 |
| ShowMapHint | `string roomName, string[] description` | 显示房间名称和多行描述 |
| ShowMapHint | `string roomName, string[] description, float time` | 显示房间名称和多行描述，自定义持续时间 |

**方法——职业提示：**

| 方法 | 参数 | 描述 |
|------|------|------|
| ShowRoleHint | `string roleName` | 仅显示职业名称（短暂持续） |
| ShowRoleHint | `string roleName, float time` | 仅显示职业名称，自定义持续时间 |
| ShowRoleHint | `string roleName, string description` | 显示职业名称和一行描述 |
| ShowRoleHint | `string roleName, string description, float time` | 显示职业名称和一行描述，自定义持续时间 |
| ShowRoleHint | `string roleName, string[] description` | 显示职业名称和多行描述 |
| ShowRoleHint | `string roleName, string[] description, float time` | 显示职业名称和多行描述，自定义持续时间 |

**方法——其他提示：**

| 方法 | 参数 | 描述 |
|------|------|------|
| ShowOtherHint | `string messages` | 以 DynamicHint 形式显示单条消息 |
| ShowOtherHint | `string messages, float time` | 显示单条消息，自定义持续时间 |
| ShowOtherHint | `string[] messages` | 显示多条消息（持续时间随数量变化） |
| ShowOtherHint | `string[] messages, float time` | 显示多条消息，自定义总持续时间 |

**使用示例：**

```csharp
var ui = PlayerUI.Get(player);
ui.CommonHint.ShowRoleHint("SCP-173", new[] { "杀死所有人类", "使用你的技能" });
ui.CommonHint.ShowMapHint("重型收容区", "大多数 SCP 生成的地方");
ui.CommonHint.ShowItemHint("钥匙卡", "用于开门");
ui.CommonHint.ShowOtherHint("服务器正在启动！");
```

---

## 扩展方法

所有扩展方法集中于此以便于参考。

### AbstractHint 扩展

> 命名空间：`HintServiceMeow.Core.Extension`

| 方法 | 扩展类型 | 参数 | 描述 |
|------|---------|------|------|
| HideAfter | `AbstractHint` | `float delay` | 在 `delay` 秒后将 `Hide` 设为 `true`。重置任何现有的隐藏计时器 |

```csharp
hint.HideAfter(5f); // 5 秒后隐藏提示
```

### PlayerDisplay 扩展

> 命名空间：`HintServiceMeow.Core.Extension`

| 方法 | 扩展类型 | 参数 | 描述 |
|------|---------|------|------|
| RemoveAfter | `PlayerDisplay` | `AbstractHint hint, float delay` | 在 `delay` 秒后从显示中移除提示。重置任何现有的移除计时器 |

```csharp
playerDisplay.RemoveAfter(hint, 10f); // 10 秒后移除提示
```

### NW Player 扩展

> 命名空间：`HintServiceMeow.Core.Extension` / `HintServiceMeow.UI.Extension`

`LabApi.Features.Wrappers.Player` 的扩展方法。

| 方法 | 命名空间 | 返回值 | 描述 |
|------|---------|--------|------|
| GetPlayerDisplay | Core | `PlayerDisplay` | 获取玩家的 PlayerDisplay |
| AddHint | Core | `void` | 向玩家显示添加提示 |
| RemoveHint | Core | `void` | 从玩家显示移除提示 |
| GetPlayerUi | UI | `PlayerUI` | 获取玩家的 PlayerUI 实例 |

```csharp
// 使用 NW player 扩展（LabApi）
LabApi.Features.Wrappers.Player player = ...;

// 获取 PlayerDisplay 并直接在玩家对象上添加提示
var hint = new Hint { Text = "来自 NW 扩展的问候！", YCoordinate = 500 };
player.AddHint(hint);

// 之后移除
player.RemoveHint(hint);

// 访问 PlayerDisplay 进行更高级的操作
PlayerDisplay pd = player.GetPlayerDisplay();
pd.ShowHint(new Hint { Text = "临时提示！" }, duration: 3f);

// 访问 PlayerUI 和 CommonHint
PlayerUI ui = player.GetPlayerUi();
ui.CommonHint.ShowRoleHint("SCP-096", new[] { "坐下哭泣", "追逐目标" });
```

### EXILED Player 扩展

> 命名空间：`HintServiceMeow.Core.Extension` / `HintServiceMeow.UI.Extension`

`Exiled.API.Features.Player` 的扩展方法。仅在 EXILED 构建中可用。

| 方法 | 命名空间 | 返回值 | 描述 |
|------|---------|--------|------|
| GetPlayerDisplay | Core | `PlayerDisplay` | 获取玩家的 PlayerDisplay |
| AddHint | Core | `void` | 向玩家显示添加提示 |
| RemoveHint | Core | `void` | 从玩家显示移除提示 |
| GetPlayerUi | UI | `PlayerUI` | 获取玩家的 PlayerUI 实例 |

```csharp
// 使用 EXILED player 扩展
Exiled.API.Features.Player player = ...;

// 获取 PlayerDisplay 并直接在玩家对象上添加提示
var hint = new Hint { Text = "来自 EXILED 扩展的问候！", YCoordinate = 500 };
player.AddHint(hint);

// 之后移除
player.RemoveHint(hint);

// 访问 PlayerDisplay 进行更高级的操作
PlayerDisplay pd = player.GetPlayerDisplay();
pd.ShowHint(new Hint { Text = "临时提示！" }, duration: 3f);

// 访问 PlayerUI 和 CommonHint
PlayerUI ui = player.GetPlayerUi();
ui.CommonHint.ShowItemHint("O5 钥匙卡", "可进入所有区域");
```

---

## Hint 内容

这些类在内部被提示用于管理其文本内容。在大多数情况下，您不需要直接与它们交互——直接使用 [AbstractHint](#abstracthint) 上的 `Text` 或 `AutoText` 属性即可。

### AbstractHintContent

> 命名空间：`HintServiceMeow.Core.Models.HintContent`

Hint 内容提供者的基类。

**事件：**

| 事件 | 类型 | 描述 |
|------|------|------|
| ContentUpdated | `UpdateHandler` | 内容更改时触发 |

**方法：**

| 方法 | 参数 | 返回值 | 描述 |
|------|------|--------|------|
| GetText | — | `string?` | 返回当前文本内容 |

---

### StringContent

> 命名空间：`HintServiceMeow.Core.Models.HintContent`

保存静态文本的内容提供者。继承自 [AbstractHintContent](#abstracthintcontent)。

**构造函数：** `StringContent(string? content)`

**属性：**

| 属性 | 类型 | 描述 |
|------|------|------|
| Text | `string?` | 静态文本。更改时触发 `ContentUpdated` |

---

### AutoContent

> 命名空间：`HintServiceMeow.Core.Models.HintContent`

定期调用委托以生成动态文本的内容提供者。继承自 [AbstractHintContent](#abstracthintcontent)。

**委托：** `delegate string TextUpdateHandler(AutoContentUpdateArg ev)`

**构造函数：** `AutoContent(TextUpdateHandler? autoText, float defaultUpdateInterval = -1)`

如果 `defaultUpdateInterval` 为负数，则默认为 `0.1` 秒。

**属性：**

| 属性 | 类型 | 描述 |
|------|------|------|
| AutoText | `TextUpdateHandler?` | 调用以生成文本的委托。重置此项也会重置下次更新时间 |

**使用示例：**

```csharp
hint.AutoText = (ev) =>
{
    ev.NextUpdateDelay = TimeSpan.FromSeconds(1); // 每 1 秒更新一次
    return $"时间：{DateTime.Now:HH:mm:ss}";
};
```

---

## 过渡动画

过渡动画在提示属性发生变化时提供平滑的动画插值。属性不再直接跳变到新值，而是通过可配置的持续时间和缓动函数逐渐过渡。

### Transition 类

> 命名空间：`HintServiceMeow.Core.Models.Transition`

定义属性变化应如何进行动画。

**属性：**

| 属性 | 类型 | 描述 |
|------|------|------|
| Duration | `float` | 动画持续时间（秒）。最小值：`0.001`。默认值：`0.5` |
| Easing | `EasingType` | 用于插值的缓动函数。默认值：`EaseInOut` |
| NormalizedCurve | `IAnimationCurve` | 自定义动画曲线（0–1 归一化）。当 `Easing` 为 `Custom` 时使用 |

**静态工厂方法：**

| 方法 | 参数 | 返回值 | 描述 |
|------|------|--------|------|
| Get | `EasingType type = EaseInOut, float duration = 0.5f` | `Transition` | 创建使用内置缓动的过渡 |
| Get | `IAnimationCurve normalizedCurve, float duration = 0.5f` | `Transition` | 创建使用自定义曲线的过渡 |

**使用示例：**

```csharp
// 使用内置缓动创建过渡
var smooth = Transition.Get(EasingType.EaseInOut, duration: 1f);
var quickFade = Transition.Get(EasingType.EaseOut, duration: 0.3f);
var linear = Transition.Get(EasingType.Linear, duration: 2f);

// 分配给提示属性
hint.YCoordinateTransition = smooth;
hint.FontSizeTransition = quickFade;
```

---

### EasingType 枚举

> 命名空间：`HintServiceMeow.Core.Enum`

定义插值曲线形状。

| 值 | 描述 |
|----|------|
| `Linear` | 从开始到结束匀速 |
| `EaseIn` | 开始慢，向末尾加速 |
| `EaseOut` | 开始快，向末尾减速 |
| `EaseInOut` | 开始和结束都慢，中间最快 |
| `Custom` | 使用 `Transition.NormalizedCurve` 自定义曲线 |

---

### Hint 过渡属性

这些属性可用于提示类型以启用动画过渡：

**`Hint` 上的属性**（除 [AbstractHint](#abstracthint) 属性之外）：

| 属性 | 类型 | 描述 |
|------|------|------|
| FontSizeTransition | `Transition?` | 当 `FontSize` 更改时应用的过渡。默认值：`null`（无动画） |
| XCoordinateTransition | `Transition?` | 当 `XCoordinate` 更改时应用的过渡。默认值：`null` |
| YCoordinateTransition | `Transition?` | 当 `YCoordinate` 更改时应用的过渡。默认值：`null` |

**`DynamicHint` 上的属性：**

| 属性 | 类型 | 描述 |
|------|------|------|
| FontSizeTransition | `Transition?` | 当 `FontSize` 更改时应用的过渡。默认值：`null`（无动画） |

> 注意：`DynamicHint` 不支持 `XCoordinateTransition` 或 `YCoordinateTransition`，因为其位置是自动管理的。

**使用示例：**

```csharp
var hint = new Hint
{
    Text = "动画提示",
    YCoordinate = 100,
    FontSize = 20,
    YCoordinateTransition = Transition.Get(EasingType.EaseInOut, 1f),
    FontSizeTransition = Transition.Get(EasingType.EaseOut, 0.5f),
};

playerDisplay.AddHint(hint);

// 之后更改属性——过渡会平滑地进行动画
hint.YCoordinate = 800; // 在 1 秒内平滑移动
hint.FontSize = 40;     // 在 0.5 秒内平滑增长
```

---

## 富文本标签助手

富文本标签助手为构建 Unity TextMeshPro 富文本标记提供类型安全的流式 API。您可以使用标签对象和运算符，而无需编写 `<color=#FF0000>text</color>` 这样的原始标签。

### RichTag 基类

> 命名空间：`HintServiceMeow.UI.Models`

所有富文本标签包装器的抽象基类。

**属性：**

| 属性 | 类型 | 描述 |
|------|------|------|
| OpenTag | `string`（只读） | 开始标签语法（例如 `<color=#FF0000>`） |
| CloseTag | `string`（只读） | 结束标签语法（例如 `</color>`） |

**方法：**

| 方法 | 参数 | 返回值 | 描述 |
|------|------|--------|------|
| Apply | `string str` | `string` | 用开始和结束标签包裹文本 |

**运算符：**

| 运算符 | 用法 | 描述 |
|--------|------|------|
| `/` | `"text" / tag` | `tag.Apply("text")` 的简写 |

**使用示例：**

```csharp
// 使用 / 运算符
string red = "Hello" / ColorTag.Red; // <color=#FF0000>Hello</color>

// 使用 Apply()
string bold = BoldTag.Bold.Apply("World"); // <b>World</b>
```

---

### 具体标签

> 命名空间：`HintServiceMeow.UI.Models.RichTags`

所有具体标签类都继承自 [RichTag](#richtag-基类)。固定行为的标签使用单例模式；参数化标签使用工厂方法。

**样式标签：**

| 标签类 | 访问器 | 输出示例 |
|--------|--------|----------|
| `BoldTag` | `BoldTag.Bold` | `<b>text</b>` |
| `ItalicsTag` | `ItalicsTag.Italics` | `<i>text</i>` |
| `UnderlineTag` | `UnderlineTag.Underline` | `<u>text</u>` |
| `StrikethroughTag` | `StrikethroughTag.Strikethrough` | `<s>text</s>` |

**大小写标签：**

| 标签类 | 访问器 | 输出示例 |
|--------|--------|----------|
| `AllcapsTag` | `AllcapsTag.Allcaps` | `<allcaps>text</allcaps>` |
| `LowercaseTag` | `LowercaseTag.Lowercase` | `<lowercase>text</lowercase>` |
| `SmallcapTag` | `SmallcapTag.Smallcap` | `<smallcaps>text</smallcaps>` |

**颜色和大小标签：**

| 标签类 | 工厂/访问器 | 描述 |
|--------|-------------|------|
| `ColorTag` | `ColorTag.Red`、`.Green`、`.Blue`、`.White`、`.Black`、`.Yellow`、`.Orange`、`.Purple`、`.Cyan`、`.Magenta`、`.Grey` | 预设颜色单例 |
| `ColorTag` | `ColorTag.Get(string value)` | 十六进制（`"#FF0000"`）或命名颜色（`"red"`） |
| `ColorTag` | `ColorTag.Get(byte r, byte g, byte b)` | RGB 值 |
| `SizeTag` | `SizeTag.Get(int pixel)` | 像素绝对大小 |
| `SizeTag` | `SizeTag.Get(string value)` | 带单位的大小（`"150%"`、`"1.5em"`） |

**间距标签：**

| 标签类 | 工厂 | 描述 |
|--------|------|------|
| `SpaceTag` | `SpaceTag.Get(string)` | 水平间距 |
| `CSpaceTag` | `CSpaceTag.Get(string)` | 字符间距 |
| `MSpaceTag` | `MSpaceTag.Get(string)` | 等宽宽度 |
| `IndentTag` | `IndentTag.Get(string)` | 首行缩进 |
| `LineIndentTag` | `LineIndentTag.Get(string)` | 全行缩进 |
| `LineHeightTag` | `LineHeightTag.Get(string)` | 行高 |

**位置标签：**

| 标签类 | 工厂 | 描述 |
|--------|------|------|
| `PosTag` | `PosTag.Get(string)` | 水平位置 |
| `MarginTag` | `MarginTag.Get(string)` | 文本边距 |
| `VOffsetTag` | `VOffsetTag.Get(string)` | 垂直偏移 |
| `RotateTag` | `RotateTag.Get(string)` | 文本旋转 |
| `WidthTag` | `WidthTag.Get(string)` | 文本区域宽度 |
| `AlignTag` | `AlignTag.Get(string)` | 文本对齐 |

**字体和外观标签：**

| 标签类 | 工厂 | 描述 |
|--------|------|------|
| `FontTag` | `FontTag.Get(string)` | 字体族 |
| `FontWeightTag` | `FontWeightTag.Get(string)` | 字体粗细 |
| `AlphaTag` | `AlphaTag.Get(string)` | 文本不透明度 |
| `MarkTag` | `MarkTag.Get(string)` | 文本高亮/背景颜色 |
| `GradientTag` | `GradientTag.Get(string)` | 颜色渐变 |

**特殊标签：**

| 标签类 | 访问器/工厂 | 描述 |
|--------|-------------|------|
| `NoParseTag` | `NoParseTag.NoParse` | 防止内部文本被解析为富文本 |
| `NoBRTag` | `NoBRTag.NoBR` | 防止标签内文本换行 |
| `BreakTag` | `BreakTag.Break` | 插入换行符 |
| `LinkTag` | `LinkTag.Get(string)` | 创建链接 ID |
| `HyperlinkTag` | `HyperlinkTag.Get(string)` | 创建超链接 |
| `SpriteTag` | `SpriteTag.Get(string)` | 插入精灵图 |

---

### 字符串扩展

> 命名空间：`HintServiceMeow.UI.Extension`

用于便捷标签应用的 `string` 扩展方法。

| 方法 | 参数 | 返回值 | 描述 |
|------|------|--------|------|
| UseTag | `this string str, RichTag tag` | `string` | 将单个标签应用于字符串 |
| UseTag | `this string str, params RichTag[] tags` | `string` | 应用多个标签（最外层优先） |

**使用示例：**

```csharp
// 单个标签
string red = "Hello".UseTag(ColorTag.Red);

// 多个标签——从最外层到最内层应用
string styled = "Fancy".UseTag(ColorTag.Get("#FF8800"), BoldTag.Bold, SizeTag.Get(30));
// 结果: <color=#FF8800><b><size=30>Fancy</size></b></color>

// 使用 / 运算符
string quick = "Quick" / ColorTag.Blue / BoldTag.Bold;
// 结果: <b><color=#0000FF>Quick</color></b>
```

---

## 分辨率适配

分辨率适配根据玩家的屏幕宽高比自动调整提示的定位，确保提示在不同屏幕尺寸上正确显示。

> 命名空间：`HintServiceMeow.Core.Enum`

### ResolutionOption 枚举

| 值 | 描述 |
|----|------|
| `None` | 不进行分辨率适配。提示使用原始坐标值 |
| `Offset` | 根据玩家的屏幕 XY 比例将左/右对齐的提示推向屏幕边缘 |

### AbstractHint 属性

| 属性 | 类型 | 描述 |
|------|------|------|
| ResolutionOption | `ResolutionOption` | 控制提示如何适配不同的屏幕分辨率。默认值：`Offset` |

当设置为 `Offset` 时，系统会监控每个玩家的屏幕分辨率，并调整左/右对齐提示的 `XCoordinate`，使其无论宽高比如何都能一致地显示在屏幕边缘。

**使用示例：**

```csharp
// 分辨率适配默认启用（Offset）
var hint = new Hint
{
    Text = "始终在边缘",
    Alignment = HintAlignment.Left,
    YCoordinate = 400,
    // ResolutionOption = ResolutionOption.Offset  // 这已经是默认值
};

// 如需固定定位，禁用分辨率适配
var fixedHint = new Hint
{
    Text = "固定位置",
    Alignment = HintAlignment.Left,
    YCoordinate = 400,
    ResolutionOption = ResolutionOption.None
};
```

---

## 模板

模板为创建和配置提示提供蓝图模式。它们支持可空属性——只有非空值才会被应用，使定义部分配置变得简单。Config 类支持 YAML 序列化以用于外部配置，而 Template 类添加了标记为 `[YamlIgnore]` 的仅代码属性。

> 命名空间：`HintServiceMeow.UI.Models.Template`

### 类层次结构

```
AbstractHintTemplate
├── HintConfig
│   └── HintTemplate
└── DynamicHintConfig
    └── DynamicHintTemplate

HintPositionConfig（独立，仅位置）
DynamicHintPositionConfig（独立，仅位置）
```

---

### AbstractHintTemplate

所有提示模板的基类。所有属性都是可空的——调用 `Apply()` 时只有非空值会被应用。

**属性：**

| 属性 | 类型 | 描述 |
|------|------|------|
| SyncSpeed | `HintSyncSpeed?` | 更新优先级 |
| FontSize | `int?` | 文本字体大小 |
| LineHeight | `float?` | 行间额外垂直间距 |
| Text | `string?` | 静态文本内容 |

---

### HintConfig

> 继承自：[AbstractHintTemplate](#abstracthinttemplate-1)

用于固定位置提示的 YAML 可序列化配置。

**属性（除 AbstractHintTemplate 之外）：**

| 属性 | 类型 | 描述 |
|------|------|------|
| XCoordinate | `float?` | 水平偏移量 |
| YCoordinate | `float?` | 垂直位置 |
| Alignment | `HintAlignment?` | 文本对齐方式 |
| YCoordinateAlign | `HintVerticalAlign?` | Y 坐标与文本的对齐方式 |

**方法：**

| 方法 | 参数 | 返回值 | 描述 |
|------|------|--------|------|
| Apply | `Hint hint` | `void` | 将所有非空属性应用到提示 |
| GetHint | — | `Hint` | 创建一个应用了所有非空属性的新 `Hint` |

---

### HintTemplate

> 继承自：[HintConfig](#hintconfig)

带有额外仅代码属性的完整模板。标记为 `[YamlIgnore]` 的属性不会被序列化。

**属性（除 HintConfig 之外）：**

| 属性 | 类型 | 可序列化 | 描述 |
|------|------|----------|------|
| Id | `string?` | 是 | 逻辑标识符 |
| Hide | `bool?` | 是 | 可见性 |
| AutoText | `AutoContent.TextUpdateHandler?` | 否 | 动态文本处理器 |
| Content | `AbstractHintContent?` | 否 | 内容提供者 |
| FontSizeTransition | `Transition?` | 否 | 字体大小动画 |
| XCoordinateTransition | `Transition?` | 否 | X 坐标动画 |
| YCoordinateTransition | `Transition?` | 否 | Y 坐标动画 |

**使用示例：**

```csharp
// 创建模板作为蓝图
var template = new HintTemplate
{
    FontSize = 25,
    YCoordinate = 700,
    Alignment = HintAlignment.Right,
    FontSizeTransition = Transition.Get(EasingType.EaseInOut, 0.5f),
};

// 从模板创建新提示
Hint hint = template.GetHint();
hint.Text = "从模板创建";
playerDisplay.AddHint(hint);

// 或将模板应用到现有提示
var existing = new Hint { Text = "已有提示" };
template.Apply(existing); // 只应用非空属性
```

---

### DynamicHintConfig

> 继承自：[AbstractHintTemplate](#abstracthinttemplate-1)

用于自动定位提示的 YAML 可序列化配置。

**属性（除 AbstractHintTemplate 之外）：**

| 属性 | 类型 | 描述 |
|------|------|------|
| TopBoundary | `float?` | 放置的上边界 |
| BottomBoundary | `float?` | 放置的下边界 |
| LeftBoundary | `float?` | 放置的左边界 |
| RightBoundary | `float?` | 放置的右边界 |
| TargetX | `float?` | 首选水平位置 |
| TargetY | `float?` | 首选垂直位置 |
| TopMargin | `float?` | 上方额外空间 |
| BottomMargin | `float?` | 下方额外空间 |
| LeftMargin | `float?` | 左侧额外空间 |
| RightMargin | `float?` | 右侧额外空间 |
| Priority | `HintPriority?` | 排列优先级 |
| Strategy | `DynamicHintStrategy?` | 无可用空间时的行为 |

**方法：**

| 方法 | 参数 | 返回值 | 描述 |
|------|------|--------|------|
| Apply | `DynamicHint hint` | `void` | 将所有非空属性应用到提示 |
| GetDynamicHint | — | `DynamicHint` | 创建一个应用了所有非空属性的新 `DynamicHint` |

---

### DynamicHintTemplate

> 继承自：[DynamicHintConfig](#dynamichintconfig)

带有额外仅代码属性的动态提示完整模板。

**属性（除 DynamicHintConfig 之外）：**

| 属性 | 类型 | 可序列化 | 描述 |
|------|------|----------|------|
| Id | `string?` | 是 | 逻辑标识符 |
| Hide | `bool?` | 是 | 可见性 |
| AutoText | `AutoContent.TextUpdateHandler?` | 否 | 动态文本处理器 |
| Content | `AbstractHintContent?` | 否 | 内容提供者 |
| FontSizeTransition | `Transition?` | 否 | 字体大小动画 |

---

### 仅位置配置

这些轻量级配置类仅包含位置相关属性。它们**不**继承自 `AbstractHintTemplate`。

**HintPositionConfig：**

| 属性 | 类型 | 描述 |
|------|------|------|
| XCoordinate | `float?` | 水平偏移量 |
| YCoordinate | `float?` | 垂直位置 |
| Alignment | `HintAlignment?` | 文本对齐方式 |
| YCoordinateAlign | `HintVerticalAlign?` | Y 坐标与文本的对齐方式 |

方法：`Apply(Hint)`、`GetHint()`

**DynamicHintPositionConfig：**

| 属性 | 类型 | 描述 |
|------|------|------|
| TopBoundary | `float?` | 上边界 |
| BottomBoundary | `float?` | 下边界 |
| LeftBoundary | `float?` | 左边界 |
| RightBoundary | `float?` | 右边界 |
| TargetX | `float?` | 首选水平位置 |
| TargetY | `float?` | 首选垂直位置 |
| TopMargin | `float?` | 上方额外空间 |
| BottomMargin | `float?` | 下方额外空间 |
| LeftMargin | `float?` | 左侧额外空间 |
| RightMargin | `float?` | 右侧额外空间 |

方法：`Apply(DynamicHint)`、`GetHint()`

点击[此处](/Docs/SimplifiedChinese/README.md)返回 README

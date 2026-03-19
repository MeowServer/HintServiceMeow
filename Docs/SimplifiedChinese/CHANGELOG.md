点击[此处](/Docs/SimplifiedChinese/README.md)返回 README

# 更新日志

本项目所有重要更改都将记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)。

---

## [未发布]

---

# [5.5.1]
### 修复
- 修复了由 Instance 属性引起的 Exiled 配置加载错误。

---

## [5.5.0]

### 修复
- 修复了 `PlayerDisplay` 在销毁后仍可能发送提示的问题。
- 修复了 `HintCollection` 在对应的 `List<Hint>` 为空时未从集合中删除键的问题。
- 修复了 `AbstractHint` 未从 `AbstractHint::content::ContentUpdated` 解绑 `AbstractHint::OnContentUpdate` 的问题。
- 修复了导致提示更新不必要延迟的问题。
- 修复了 `AutoContent::autoText` 持续抛出异常时 `AutoContent` 不断调用函数的问题。
- 修复了 `PlayerDisplay::Destruct()` 中可能引发 `NullReferenceException` 的问题。
- 修复了 `TaskScheduler` 中可能导致线程问题的多个 bug。
- 修复了 `AbstractHint`、`Hint` 和 `DynamicHint` 中当同一 `PlayerDisplay` 的多个实例同时更新时引发死锁的问题。
- 修复了 CommonHint 中某些方法使用错误默认显示时间的问题。

### 更改
- 将 `CompatibilityAdaptor` 默认设为禁用以确保安全性。
- 将依赖项移至 GitHub 仓库。
- 根据代码风格限制改进了代码风格。
- 改进了 `PlayerDisplay::ScheduleUpdate(float, AbstractHint?)` 的性能。
- 将 `Hints.HintEffectPresets.TrailingPulseAlpha(1, 1, 1)` 替换为 `Hints.AlphaEffect(1)` 以提高性能。
- 重写了 `DefaultDisplayOutput` 和 `Patches` 以确保与新版本的兼容性。
- 对 `PlayerDisplay` 进行了微小调整以防止关键稳定性问题。
- 改进了 `HintServiceMeow.Test` 中测试的命名和代码风格。
- 在 `PlayerDisplay` 和 `CompatibilityAdaptor` 中将 `MEC` 替换为 `ICoroutine` 以消除 Unity 依赖。
- 使 `RichTextParserPool` 在返回 `RichTextParser` 前清除状态。
- 使 `HintParser` 独立于 Mirror。
- 提升了 `HintParser` 的性能。执行速度最高提升了 32%，内存分配减少了高达 76%。
- 改进了 `HintServiceExample` 以提供更详细和全面的演示。
- 改进了 `HintExtension` 和 `PlayerDisplayExtension` 的性能。

### 新增
- 新增 `PlayerDisplay::AddHint(params AbstractHint[])`、`RemoveHint(params AbstractHint[])`、`SetMinUpdateInterval(TimeSpan)`、`AddHint(AbstractHint?, string)`、`RemoveHint(AbstractHint?, string)`、`ShowHint(AbstractHint, float, AfterShowAction)` 和 `ShowHint(IEnumerable<AbstractHint>, float, AfterShowAction)`。
- 使用 Release 模式时强制执行代码风格限制。（感谢 @Someone）
- 添加了 Bug 报告模板、功能请求模板和行为准则文件。
- 在 `HintCollection::AllGroups` 和 `HintCollection::AllHints` 中添加了缓存。
- 在 `PlayerDisplay` 中添加了调试日志。
- 为 `PlayerDisplay`、`Cache`、`PeriodicRunner`、`UpdateAnalyzer`、`DynamicHint`、`HintCollection`、`Hint`、`CompatibilityAdaptor`、`HintParser`、`RichTextParser`、`ConcurrentTaskDispatcher`、`CoordinateTools`、`AutoContent`、`StringContent`、`Patcher`（尚未激活）、`Patches`（尚未激活）、`RichTextParserTool` 和 `StringBuilderPool` 添加了单元测试。与 Harmony 补丁相关的测试未激活，因为 Harmony 无法在测试环境中运行。
- 添加了 `HintServiceMeow.Benchmark` 以测量 `HintServiceMeow` 的性能。

---

## [5.4.4]

### 修复
- 修复了 `PlayerDisplay::CoroutineMethod` 无法正常工作的问题

---

## [5.4.3]

### 修复
- 修复了 `CoordinateTool::GetTextWidth` 处理空字符串时抛出异常的问题

---

## [5.4.2]

### 修复
- 修复了兼容性适配器（CA）无法正确清除提示的问题

---

## [5.4.1]

### 更改
- 集中化多线程操作以提高性能

### 移除
- 移除 YamlDotNet 依赖以获得更好的兼容性

---

## [5.4.0]

### 更改
- **破坏性更改：** 更新了 `AutoText` 参数

---

## [5.4.0-beta.2]

### 修复
- 修复了部分代码错误地使用 PluginAPI 而非 LabAPI 的问题

---

## [5.4.0-beta.1]

### 新增
- 添加了对 LabAPI 的支持

### 移除
- 移除了对 NWAPI 的支持
- 移除了提示更新频率限制

### 修复
- 修复了多个 bug

---

## [5.3.14]

### 修复
- 修复了 `PlayerDisplay` 销毁后 `AutoText` 继续调用的问题

---

## [5.3.13]

### 修复
- 修复了多个 bug

---

## [5.3.12]

### 修复
- 修复了 `PlayerDisplay` 中 `RemoveAfter` 无法正常工作的问题
- 微小调整以防止 bug

### 移除
- 移除了溢出检测（无法正常工作——请根据需要手动换行）

---

## [5.3.11]

### 修复
- 修复了 `PlayerDisplay` 中按 ID 移除无法正常工作的问题
- 修复了 `HintContent` 中的命名空间错误——**注意：这可能会破坏使用 `HintContents` 的插件**

---

## [5.3.10]

### 更改
- 标准化了代码风格
- 重新实现了插件框架的 API
- 微小性能调整

### 修复
- 修复了导致提示卡在屏幕上的问题

---

## [5.3.9]

### 新增
- 添加了对 `\n`（纯文本）作为换行符的支持

### 修复
- 修复了 `RichTextParser` 中 `<br>` 换行标签无法正常工作的问题

---

## [5.3.8]

### 更改
- 将 `MultiThreadTool` 替换为 `MainThreadDispatcher`

### 修复
- 修复了 `StringBuilderPool` 中可能导致内存泄漏的问题

---

## [5.3.7]

### 新增
- 在 `PlayerDisplay.StartParserTask` 中添加了错误处理
- 添加了对文本中 `<br>` 标签的支持

---

## [5.3.6]

### 新增
- 在 `TextUpdateArg` 中添加了延迟时间属性

### 更改
- 改进了代码质量

### 修复
- 修复了 `PlayerDisplay` 中的线程安全问题
- 修复了多个问题

---

## [5.3.5]

### 新增
- 在 `PlayerDisplay` 中添加了更多可自定义的属性

### 更改
- 提高了兼容性适配器的稳定性
- 提升了性能
- 微小代码质量改进

### 修复
- 修复了可能导致更新频率高于预期的问题
- 修复了扩展中的多个 bug
- 修复了提示集合中的线程安全问题
- 修复了导致 Linux 系统崩溃的问题

---

## [5.3.4]

### 修复
- 修复了传入负数 `Duration` 时 `CompatibilityAdapter` 出现的问题
- 修复了 `TaskScheduler` 中的线程安全问题
- 修复了 `FontTool` 中的问题

---

## [5.3.3]

### 更改
- 改进了代码质量

### 修复
- 修复了 `Timing.CallDelayed` 中的问题

---

## [5.3.2]

### 修复
- 修复了 `CompatibilityAdapter` 无法正常工作的问题
- 修复了更新管理无法正常工作的问题

---

## [5.3.1]

### 新增
- 为 `PlayerDisplay` 和 `AbstractHint` 添加了 `RemoveAfter` 和 `HideAfter` 属性

### 更改
- 重写了 `PlayerDisplay` 中的更新管理代码

---

## [5.3.0]

### 新增
- 添加了字符串构建器池以提高性能

### 更改
- 使用 .NET 4.8（而非 4.8.1）作为默认版本
- 改进了 NW API 兼容性
- 微小命名更新

### 修复
- 修复了导致 `ReceiveHint` 补丁崩溃的问题

---

## [5.3.0-pre.2.3]

### 修复
- 修复了计算文本高度时未包含行高的问题
- 修复了 `FontTools` 未正确计算字符长度的问题
- 修复了 `RichTextParser` 未正确处理换行符的问题

---

## [5.3.0-pre.2.2]

### 修复
- 修复了导致行高不可用的问题
- 微小更新和 bug 修复

---

## [5.3.0-pre.2.1]

### 新增
- 添加了对 `case` 样式和 `script` 样式标签的支持
- 在 DynamicHint 中添加了 `margin` 属性

### 修复
- 修复了富文本解析器处理对齐方式不正确的问题
- 修复了富文本解析器换行不正确的问题

---

## [5.3.0-pre.2.0]

### 新增
- 添加了对提示中 `size` 标签的支持

### 更改
- 改进了兼容性适配器的行为

### 修复
- 修复了获取玩家显示时导致服务器崩溃的问题

---

## [5.3.0-pre.1.4]

### 修复
- 修复了 DynamicHint 显示不正确的问题
- 修复了 `PlayerDisplay` 中的空引用问题
- 修复了空行处理不正确的问题

---

## [5.3.0-pre.1.3]

### 修复
- 修复了兼容性适配器提示闪烁的问题
- 修复了多行提示显示不正确的问题

---

## [5.3.0-pre.1.2]

### 更改
- 改进了 `HintParser` 的行为
- 改进了线程安全性

---

## [5.3.0-pre.1.1]

### 新增
- 在兼容性适配器的 `line-height` 中添加了对 `em` 单位的支持

### 修复
- 修复了 `Style` 组件颜色不起作用的问题
- 修复了兼容性适配器中 `pos` 标签不起作用的问题

---

## [5.3.0-pre.1.0]

### 新增
- 为核心函数添加了多线程支持
- 在兼容性适配器中添加了 `pos` 标签支持
- 在 PlayerUI 中添加了 `Style` 组件

---

## [5.2.5]

### 修复
- 修复了兼容性适配器缓存可能导致高内存使用的问题

---

## [5.2.4]

### 新增
- 在兼容性适配器中添加了对 `color`、`b`、`i` 标签的支持
- 为 `PlayerDisplay` 添加了更多方法

---

## [5.2.3]

### 修复
- 改进了兼容性适配器的精度；解决了字体大小问题

---

## [5.2.2]

### 更改
- 性能改进
- 改进了代码质量

### 修复
- 修复了多个 bug

---

## [5.2.1]

### 修复
- 修复了兼容性适配器配置未被应用的问题

---

## [5.2.0]

### 新增
- 添加了兼容性适配器

### 更改
- 性能改进

---

## [5.1.2]

### 新增
- 为所有提示添加了 `LineHeight` 属性

### 更改
- 调整了同步速度以改善显示性能

---

## [5.1.1]

### 修复
- 修复了文本长度计算不正确的问题

---

## [5.1.0]

### 新增
- 添加了对文本中 `\n` 的支持

### 更改
- 改进了 `DynamicHint` 的性能

---

## [5.0.2]

### 修复
- 修复了多个 bug

---

## [5.0.1]

### 更改
- 改进了字体安装的体验

### 修复
- 修复了 DynamicHint 排列中的问题

---

## [5.0.0]

### 新增
- 添加了同步速度、自动文本和多个新的提示属性
- 添加了 NW API 支持

### 更改
- 重写了核心代码
- 标准化了代码风格
- 将 PlayerUI 和 CommonHint 分离

### 移除
- 移除了提示配置模板

### 修复
- 修复了导致字体文件被放置在 TEMP 文件夹的问题
- 修复了阻止 NW API 正确加载插件的问题

---

## [4.0.0]

### 新增
- 为提示添加了配置类
- 在 `PlayerDisplay` 中添加了刷新事件
- 添加了提示优先级
- 添加了可自定义的通用提示

### 更改
- 改进了代码质量

---

## [3.3.0]

### 更改
- 将 `PlayerUITemplate` 从 `PlayerUIConfig` 分离到新插件 `CustomizableUIMeow`

---

## [3.2.0]

### 更改
- 整理了配置
- 使 `PlayerUIConfig` 更具可自定义性

---

## [3.1.2]

### 更改
- 使用补丁阻止来自其他插件的所有提示

---

## [3.1.1]

### 修复
- 修复了多个 bug

---

## [3.1.0]

### 新增
- 添加了 `PlayerUIConfig` 配置

---

## [3.0.2]

### 修复
- 修复了当屏幕上没有提示时 `PlayerDisplay` 崩溃的问题

---

## [3.0.1]

### 修复
- 修复了多个 bug

---

## [3.0.0]

### 更改
- 将 `ReferenceHub UI` 从 `PlayerDisplay` 分离并扩展了更多方法

---

## [2.2.0]

### 更改
- 使用事件更新 `ReferenceHub` 显示，提高了稳定性并降低了开销

---

## [2.1.1]

### 修复
- 修复了多个 bug

---

## [2.1.0]

### 新增
- 添加了通用提示

---

## [2.0.0]

### 新增
- 添加了 DynamicHint 支持
- 添加了最大更新频率限制（0.5 次/秒）

### 修复
- 修复了多个 bug

---

## [1.0.1]

### 更改
- 根据提示内容更新来刷新显示

---

## [1.0.0]

### 新增
- 初始发布

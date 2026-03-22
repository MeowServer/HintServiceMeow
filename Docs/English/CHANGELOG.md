Click [here](/Docs/English/README.md) to go back to read me

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased]
### Added
#### Transition
- Animation and hint parameter support in `Hint` and `DynamicHint`. 
- `AbstractHint::FontSizeTransition`, `Hint::XCoordianteTransition`, `Hint::YCoordianteTransition`, `DynamicHint::XCoordianteTransition`, `DynamicHint::YCoordianteTransition`.
- `Transition` and `TransitionState` classes
#### Hint Config and Template
- Config and templates for `Hint` and `DynamicHint`. Config can be used for YAML serialization, while template can be used to quickly replicate hints with same properties.
- `AbstractHintTemplate`, `DynamicHintConfig`, `DynamicHintPositionConfig`, `DynamicHintTempalte`, `HintConfig`, `HintPositionConfig`, and `HintTemplate` Classes.
#### More extensions
- Added comprehensive extension methods for Player classes of both Exiled and LabAPI.
- Added AddHint overloads to support adding multiple hints simultaneously via IEnumerable or params arrays.
- Added hint retrieval (GetHint, TryGetHint, GetHints), removal(RemoveHint, RemoveHints), and limited-time showing(ShowHint, ShowHints) player extension method.
- Added CommonHint(Item hint, map hint, role hint, and other hint) player extension methods.
#### RichTagHelper
- Added `RichTag` class and extension methods to help with building unity rich text.
- Added extensino for `string` and `StringBuilder` for more convenient rich text writing experience.
- Added / operator to apply a tag to a `string` instance.
#### Resolution Adaption
- Added adaption to different screen size.
- Added `AbstractHint::ResolutionOption`.

### Changed
#### Better RichTextParser
- Rewrote RichTextParser to increase compatibility with tags.
- Added `LineStyle`, `TextMeshStyle`, `TextSegmentStyle`, `Color`, `MeasuredValue`, `LineInfo`, `TextSegment`, `Token`, `RichTextParserResult`, `RichTextParserSetting`,  model classes.
- Addeed `TagChecker`, `Tokenizer` utility classes.
- Rewrote `RichTextParser`.

#### Better CommonHint
- Removed redundant API.
- Add auto expand.

#### Minor Changes
- `AbstractHint::LineHeight` can be negative.
- Replaced return type of `PlayerDisplay::GetHints` with `AbstractHint[]`. Use `float maxDelay` as the parameter of `PlayerDisplay::ForceUpdate`.

---

## [5.5.1]

### Fixed
- Exiled config loading error caused by Instance property.

---
## [5.5.0]

### Fixed
- A bug that could cause `PlayerDisplay` to send hint after it was destructed.
- A bug that caused `HintCollection` not to remove key from the collection when its corresponding `List<Hint>` was empty.
- A bug that caused `AbstractHint` not to unbind `AbstractHint::OnContentUpdate` from `AbstractHint::content::ContentUpdated`.
- A bug that caused unnecessary delay in hint update.
- A bug that caused `AutoContent` to keep calling the function when `AutoContent::autoText` kept throwing exceptions.
- A bug that could potentially cause `NullReferenceException` in `PlayerDisplay::Destruct()`.
- Bugs that could cause thread issues in `TaskScheduler`.
- A bug in `AbstractHint`, `Hint`, and `DynamicHint` that caused deadlock when multiple instances in the same `PlayerDisplay` were updated at the same time.
- A bug that caused some methods in common hint to use wrong default display time.

### Changed
- Set `CompatibilityAdaptor` to disabled by default to ensure safety.
- Moved dependencies to the GitHub repo.
- Improved code style based on code style restrictions.
- Improved performance of `PlayerDisplay::ScheduleUpdate(float, AbstractHint?)`.
- Replaced `Hints.HintEffectPresets.TrailingPulseAlpha(1, 1, 1)` with `Hints.AlphaEffect(1)` to improve performance.
- Rewrote `DefaultDisplayOutput` and `Patches` to ensure compatibility with the new version.
- Made minor adjustments in `PlayerDisplay` to prevent critical stability issues.
- Improved tests naming and code style in `HintServiceMeow.Test`.
- Replaced `MEC` with `ICoroutine` in `PlayerDisplay` and `CompatibilityAdaptor` to remove the Unity dependency.
- Made `RichTextParserPool` to clear status before returning a `RichTextParser`.
- Made `HintParser` independent of Mirror.
- Improved `HintParser`'s performance. Achieved up to 32% faster execution and reducing memory allocation by up to 76%.
- Improved `HintServiceExample` for a more detailed and comprehensive demonstration.
- Improved performance of `HintExtension` and `PlayerDisplayExtension`.

### Added
- `PlayerDisplay::AddHint(params AbstractHint[])`, `RemoveHint(params AbstractHint[])`, `SetMinUpdateInterval(TimeSpan)`, `AddHint(AbstractHint?, string)`, `RemoveHint(AbstractHint?, string)`, `ShowHint(AbstractHint, float, AfterShowAction)`, and `ShowHint(IEnumerable<AbstractHint>, float, AfterShowAction)`.
- Mandatory code style restrictions when using Release mode. (Thanks to @Someone)
- Bug report template, feature request template, and Code of Conduct file.
- Cache in `HintCollection::AllGroups` and `HintCollection::AllHints`.
- Debug log to `PlayerDisplay`.
- Unit tests to `PlayerDisplay`, `Cache`, `PeriodicRunner`, `UpdateAnalyzer`, `DynamicHint`, `HintCollection`, `Hint`, `CompatibilityAdaptor`, `HintParser`, `RichTextParser`, `ConcurrentTaskDispatcher`, `CoordinateTools`, `AutoContent`, `StringContent`, `Patcher` (not activated yet), `Patches` (not activated yet), `RichTextParserTool`, and `StringBuilderPool`. Tests related to Harmony patches are not activated since Harmony cannot run in a test environment.
- `HintServiceMeow.Benchmark` to measure the performance of `HintServiceMeow`.

---

## [5.4.4]

### Fixed
- Bug causing `PlayerDisplay::CoroutineMethod` not working correctly

---

## [5.4.3]

### Fixed
- Bug causing `CoordinateTool::GetTextWidth` to throw an exception when handling empty strings

---

## [5.4.2]

### Fixed
- Bug causing the Compatibility Adapter (CA) not to clear hints correctly

---

## [5.4.1]

### Changed
- Centralized multi-thread actions to improve performance

### Removed
- YamlDotNet dependency for better compatibility

---

## [5.4.0]

### Changed
- **Breaking:** `AutoText` parameter updated

---

## [5.4.0-beta.2]

### Fixed
- Part of code mistakenly using PluginAPI instead of LabAPI

---

## [5.4.0-beta.1]

### Added
- Support for LabAPI

### Removed
- Support for NWAPI
- Hint update frequency limitation

### Fixed
- Various bugs

---

## [5.3.14]

### Fixed
- `AutoText` continuing to call after `PlayerDisplay` is destructed

---

## [5.3.13]

### Fixed
- Various bugs

---

## [5.3.12]

### Fixed
- Bug causing `RemoveAfter` in `PlayerDisplay` not working correctly
- Minor adjustments to prevent bugs

### Removed
- Overflow detection (not working correctly — please break lines manually as needed)

---

## [5.3.11]

### Fixed
- Bug in `PlayerDisplay` causing removal by ID not working correctly
- Namespace error in `HintContent` — **note: this may break plugins using `HintContents`**

---

## [5.3.10]

### Changed
- Standardized code style
- Re-implemented API for plugin frameworks
- Minor performance adjustments

### Fixed
- Bug causing hints to become stuck on screen

---

## [5.3.9]

### Added
- Support for `\n` (plain text) as a line break

### Fixed
- Issue in `RichTextParser` causing `<br>` line break tag not working correctly

---

## [5.3.8]

### Changed
- Replaced `MultiThreadTool` with `MainThreadDispatcher`

### Fixed
- Issue in `StringBuilderPool` that could cause memory leaks

---

## [5.3.7]

### Added
- Error handling in `PlayerDisplay.StartParserTask`
- Support for `<br>` tag in text

---

## [5.3.6]

### Added
- Delay time property in `TextUpdateArg`

### Changed
- Improved code quality

### Fixed
- Thread safety issue in `PlayerDisplay`
- Various issues

---

## [5.3.5]

### Added
- More customizable properties in `PlayerDisplay`

### Changed
- Improved stability of Compatibility Adapter
- Improved performance
- Minor code quality improvements

### Fixed
- Bug that could cause update rate to be higher than expected
- Various bugs in extensions
- Thread safety issues in Hint Collection
- Bug causing crash on Linux systems

---

## [5.3.4]

### Fixed
- Bug in `CompatibilityAdapter` appearing when negative `Duration` is passed
- Thread safety issue in `TaskScheduler`
- Issue in `FontTool`

---

## [5.3.3]

### Changed
- Improved code quality

### Fixed
- Issue in `Timing.CallDelayed`

---

## [5.3.2]

### Fixed
- Bug causing `CompatibilityAdapter` not working correctly
- Bug causing update management not working correctly

---

## [5.3.1]

### Added
- `RemoveAfter` and `HideAfter` properties to `PlayerDisplay` and `AbstractHint`

### Changed
- Rewrote update management code in `PlayerDisplay`

---

## [5.3.0]

### Added
- String builder pool to improve performance

### Changed
- Use .NET 4.8 (instead of 4.8.1) as default version
- Improved NW API compatibility
- Minor naming updates

### Fixed
- Problem causing `ReceiveHint` patch to crash

---

## [5.3.0-pre.2.3]

### Fixed
- Line height not being included when calculating text height
- `FontTools` not calculating character length correctly
- `RichTextParser` not handling line breaks correctly

---

## [5.3.0-pre.2.2]

### Fixed
- Bug causing line height not to be usable
- Minor updates and bug fixes

---

## [5.3.0-pre.2.1]

### Added
- Support for `case` style and `script` style tags
- `margin` properties in Dynamic Hint

### Fixed
- Bug causing rich text parser to handle alignment incorrectly
- Bug causing rich text parser to break lines incorrectly

---

## [5.3.0-pre.2.0]

### Added
- Support for `size` tag in hints

### Changed
- Improved behavior of the Compatibility Adapter

### Fixed
- Bug causing server crash when getting the player display

---

## [5.3.0-pre.1.4]

### Fixed
- Bug causing Dynamic Hint to be displayed incorrectly
- Null reference problem in `PlayerDisplay`
- Bug causing empty lines to be handled incorrectly

---

## [5.3.0-pre.1.3]

### Fixed
- Bug causing Compatibility Adapter hints to flicker
- Bug causing multi-line hints not displayed correctly

---

## [5.3.0-pre.1.2]

### Changed
- Improved `HintParser` behavior
- Improved thread safety

---

## [5.3.0-pre.1.1]

### Added
- Support for `em` unit in `line-height` for Compatibility Adapter

### Fixed
- Bug causing `Style` component color not working
- Bug causing `pos` tag in Compatibility Adapter not working

---

## [5.3.0-pre.1.0]

### Added
- Multi-thread support for core functions
- `pos` tag support in Compatibility Adapter
- `Style` component in PlayerUI

---

## [5.2.5]

### Fixed
- Compatibility Adapter cache potentially causing high memory usage

---

## [5.2.4]

### Added
- Support for `color`, `b`, `i` tags in Compatibility Adapter
- More methods to `PlayerDisplay`

---

## [5.2.3]

### Fixed
- Compatibility Adapter accuracy improved; font size issue resolved

---

## [5.2.2]

### Changed
- Performance improvements
- Improved code quality

### Fixed
- Various bugs

---

## [5.2.1]

### Fixed
- Bug causing config not being applied for Compatibility Adapter

---

## [5.2.0]

### Added
- Compatibility Adapter

### Changed
- Performance improvements

---

## [5.1.2]

### Added
- `LineHeight` property for all hints

### Changed
- Adjusted sync speed to improve display performance

---

## [5.1.1]

### Fixed
- Bug causing text length to be calculated incorrectly

---

## [5.1.0]

### Added
- Support for `\n` in text

### Changed
- Improved `DynamicHint` performance

---

## [5.0.2]

### Fixed
- Various bugs

---

## [5.0.1]

### Changed
- Improved experience in font installation

### Fixed
- Bug in Dynamic Hint arranging

---

## [5.0.0]

### Added
- Sync speed, auto text, and several new hint properties
- NW API support

### Changed
- Rewrote core code
- Standardized code style
- Separated PlayerUI and CommonHint

### Removed
- Hint config template

### Fixed
- Bug causing font file to be placed in the TEMP folder
- Bug preventing NW API from loading the plugin correctly

---

## [4.0.0]

### Added
- Config class for hints
- Refresh event in `PlayerDisplay`
- Hint priority
- Customizable common hints

### Changed
- Improved code quality

---

## [3.3.0]

### Changed
- Separated `PlayerUITemplate` from `PlayerUIConfig` into a new plugin: `CustomizableUIMeow`

---

## [3.2.0]

### Changed
- Organized config
- Made `PlayerUIConfig` more customizable

---

## [3.1.2]

### Changed
- Used patch to block all hints from other plugins

---

## [3.1.1]

### Fixed
- Various bugs

---

## [3.1.0]

### Added
- `PlayerUIConfig` config

---

## [3.0.2]

### Fixed
- Bug causing `PlayerDisplay` to crash when no hint is displayed on screen

---

## [3.0.1]

### Fixed
- Various bugs

---

## [3.0.0]

### Changed
- `ReferenceHub UI` separated from `PlayerDisplay` and extended with more methods

---

## [2.2.0]

### Changed
- Use events to update `ReferenceHub` display, increasing stability and decreasing costs

---

## [2.1.1]

### Fixed
- Various bugs

---

## [2.1.0]

### Added
- Common Hints

---

## [2.0.0]

### Added
- Dynamic Hint support
- Maximum update rate limit (0.5/second)

### Fixed
- Various bugs

---

## [1.0.1]

### Changed
- Updated display based on hint content updates

---

## [1.0.0]

### Added
- Initial release
Click [here](/Docs/English/README.md) to go back to read me

# Core Features

This documentation covers the public API of HintServiceMeow, organized by functional module.

---

## Table of Contents

- [Core Features](#core-features)
  - [Table of Contents](#table-of-contents)
  - [Hint Models](#hint-models)
    - [AbstractHint](#abstracthint)
    - [Hint](#hint)
    - [DynamicHint](#dynamichint)
  - [PlayerDisplay](#playerdisplay)
  - [UI Layer](#ui-layer)
    - [PlayerUI](#playerui)
    - [CommonHint](#commonhint)
  - [Extension Methods](#extension-methods)
    - [AbstractHint Extensions](#abstracthint-extensions)
    - [PlayerDisplay Extensions](#playerdisplay-extensions)
    - [NW Player Extensions](#nw-player-extensions)
    - [EXILED Player Extensions](#exiled-player-extensions)
  - [Hint Content](#hint-content)
    - [AbstractHintContent](#abstracthintcontent)
    - [StringContent](#stringcontent)
    - [AutoContent](#autocontent)

---

## Hint Models

### AbstractHint

> Namespace: `HintServiceMeow.Core.Models.Hints`

Base class for all hint types. **All hints (`Hint`, `DynamicHint`, etc.) inherit from `AbstractHint`**, which provides common properties such as text content, font size, sync speed, and visibility. It implements `INotifyPropertyChanged` so that property changes automatically trigger display updates.

When creating any kind of hint, the properties listed below are always available regardless of the specific hint type.

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| Guid | `Guid` (readonly) | Auto-generated unique identifier |
| Id | `string` | Custom string identifier for lookup. Default: `""` |
| SyncSpeed | `HintSyncSpeed` | Update priority. Default: `Normal`. Values: `Fastest` (192) — updates as soon as possible, may delay other hints; `Fast` (160) — plans an update immediately on change; `Normal` (128) — standard speed; `Slow` (96) — waits for other hints first; `Slowest` (64) — waits longer; `UnSync` (32) — no auto-sync, still updates when other hints trigger a sync |
| FontSize | `int` | Text font size. Default: `20` |
| LineHeight | `float` | Extra vertical spacing between lines |
| Content | `AbstractHintContent` | The content provider for this hint. Default: `StringContent("")` |
| Text | `string?` | Shortcut to get/set static text. Setting this replaces `Content` with a new `StringContent` |
| AutoText | `AutoContent.TextUpdateHandler?` | Shortcut to get/set a dynamic text delegate. Setting this replaces `Content` with a new `AutoContent` |
| PreserveCase | `bool` | Preserve mixed-case text instead of the game's default small-caps hint rendering. Default: `false` |
| Hide | `bool` | Whether the hint is hidden. Default: `false` |

**Usage Example:**

```csharp
// Properties auto-sync to the player's screen
hint.Text = "Updated text";
hint.FontSize = 30;
hint.PreserveCase = true;
// No additional method calls needed
```

`PreserveCase` only changes the text sent for measurement and rendering; it does not mutate `Text` or `Content`. Existing `<lowercase>`, `<uppercase>`, `<allcaps>`, and `<smallcaps>` regions remain authoritative. Content inside `<noparse>` is not rewritten because injected tags would be displayed literally there.

---

### Hint

> Namespace: `HintServiceMeow.Core.Models.Hints`

A fixed-position hint displayed at specific screen coordinates. Inherits from [AbstractHint](#abstracthint).

**Properties (in addition to AbstractHint):**

| Property | Type | Description |
|----------|------|-------------|
| YCoordinate | `float` | Vertical position. Higher values move the text lower on screen. Default: `700` |
| XCoordinate | `float` | Horizontal offset. Higher values move the text to the right. Default: `0` |
| Alignment | `HintAlignment` | Text alignment. Values: `Left`, `Right`, `Center`. Default: `Center` |
| YCoordinateAlign | `HintVerticalAlign` | How Y coordinate aligns to the text. Values: `Top` — Y is the top edge; `Middle` — Y is the vertical center; `Bottom` — Y is the bottom edge. Default: `Middle` |

![Y Coordinate Example](Images/YCoordinateExample.jpg)

**Usage Example:**

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

Since HSM has an auto-update feature, any changes to a property will automatically reflect on the player's screen without any further method calls.

```csharp
hint.Text = "Some New Text";
// No additional method calls needed
```

---

### DynamicHint

> Namespace: `HintServiceMeow.Core.Models.Hints`

A hint that is automatically positioned to avoid overlapping with other hints. Inherits from [AbstractHint](#abstracthint).

**Properties (in addition to AbstractHint):**

| Property | Type | Description |
|----------|------|-------------|
| TopBoundary | `float` | Upper boundary for placement. Default: `0` |
| BottomBoundary | `float` | Lower boundary for placement. Default: `1000` |
| LeftBoundary | `float` | Left boundary for placement. Default: `-1200` |
| RightBoundary | `float` | Right boundary for placement. Default: `1200` |
| TargetX | `float` | Preferred horizontal position. Default: `0` |
| TargetY | `float` | Preferred vertical position. Default: `700` |
| TopMargin | `float` | Extra space above the hint during arrangement. Default: `5` |
| BottomMargin | `float` | Extra space below the hint during arrangement. Default: `5` |
| LeftMargin | `float` | Extra space to the left during arrangement. Default: `100` |
| RightMargin | `float` | Extra space to the right during arrangement. Default: `100` |
| Priority | `HintPriority` | Arrangement priority. Higher priority hints are arranged first. Values: `Highest` (192), `High` (160), `Medium` (128), `Low` (96), `Lowest` (64). Default: `Medium` |
| Strategy | `DynamicHintStrategy` | Behavior when no space is available. Values: `Hide` — hide the hint; `StayInPosition` — keep at target position. Default: `Hide` |

**Usage Example:**

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

> Namespace: `HintServiceMeow.Core.Utilities`

The central class for managing a player's hint display. Each player has one `PlayerDisplay` instance.

**Events:**

| Event | Type | Description |
|-------|------|-------------|
| UpdateAvailable | `UpdateAvailableEventHandler` | Raised each tick when the display is ready to update |

**Delegate:** `delegate void UpdateAvailableEventHandler(UpdateAvailableEventArg ev)`

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| ReferenceHub | `ReferenceHub?` (readonly) | The player this display belongs to |
| HintParser | `IHintParser` | The parser that converts hints to rich text. Replaceable |
| CompatibilityAdaptor | `ICompatibilityAdaptor` | The adaptor for compatibility with other plugins. Replaceable |

**Static Methods:**

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| Get | `ReferenceHub referenceHub` | `PlayerDisplay` | Gets or creates a PlayerDisplay for the player |
| Get | `LabApi.Features.Wrappers.Player player` | `PlayerDisplay` | Gets or creates a PlayerDisplay (NW/LabApi) |
| Get | `Exiled.API.Features.Player player` | `PlayerDisplay` | Gets or creates a PlayerDisplay (EXILED only) |

**Instance Methods:**

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| AddHint | `AbstractHint? hint` | `void` | Adds a hint to the display |
| AddHint | `IEnumerable<AbstractHint>? hints` | `void` | Adds multiple hints |
| AddHint | `params AbstractHint[]? hints` | `void` | Adds multiple hints (params) |
| AddHint | `AbstractHint? hint, string groupName` | `void` | Adds a hint to a specific group |
| ShowHint | `AbstractHint hint, float duration = 7f, AfterShowAction afterShow = AfterShowAction.Remove` | `void` | Adds a hint and automatically removes/hides it after `duration` seconds. `AfterShowAction` values: `Remove` — remove the hint; `Hide` — set `Hide = true` |
| ShowHint | `IEnumerable<AbstractHint> hints, float duration = 7f, AfterShowAction afterShow = AfterShowAction.Remove` | `void` | Shows multiple hints with auto-removal |
| RemoveHint | `AbstractHint? hint` | `void` | Removes a hint |
| RemoveHint | `IEnumerable<AbstractHint>? hints` | `void` | Removes multiple hints |
| RemoveHint | `params AbstractHint[]? hints` | `void` | Removes multiple hints (params) |
| RemoveHint | `AbstractHint? hint, string groupName` | `void` | Removes a hint from a specific group |
| RemoveHint | `string id` | `void` | Removes all hints matching the given Id |
| RemoveHint | `Guid id` | `void` | Removes the hint matching the given Guid |
| ClearHint | — | `void` | Removes all hints owned by the calling assembly |
| GetHint | `string? id` | `AbstractHint?` | Returns the first hint matching the Id |
| GetHint | `Guid guid` | `AbstractHint?` | Returns the first hint matching the Guid |
| GetHints | `string id` | `IEnumerable<AbstractHint>` | Returns all hints matching the Id |
| GetHints | — | `IEnumerable<AbstractHint>` | Returns all hints owned by the calling assembly |
| HasHint | `string id` | `bool` | Checks if any hint with the given Id exists |
| HasHint | `Guid guid` | `bool` | Checks if a hint with the given Guid exists |
| TryGetHint | `string id, out AbstractHint hint` | `bool` | Tries to get the first hint matching the Id |
| TryGetHint | `Guid guid, out AbstractHint hint` | `bool` | Tries to get the first hint matching the Guid |
| TryGetHints | `string? id, out IEnumerable<AbstractHint> hints` | `bool` | Tries to get all hints matching the Id |
| ForceUpdate | `bool useFastUpdate = false` | `void` | Forces a display update. Use when working with `HintSyncSpeed.UnSync` |
| SetMinUpdateInterval | `TimeSpan interval` | `void` | Sets the minimum interval between updates |
| AddDisplayOutput | `IDisplayOutput output` | `void` | Adds a custom display output |
| RemoveDisplayOutput | `IDisplayOutput output` | `void` | Removes a display output |
| RemoveDisplayOutput\<T\> | — | `void` | Removes all display outputs of type `T` (where `T : IDisplayOutput`) |

**Usage Example:**

```csharp
PlayerDisplay pd = PlayerDisplay.Get(player);

// Add a hint
var hint = new Hint { Text = "Hello", YCoordinate = 500 };
pd.AddHint(hint);

// Show a temporary hint for 5 seconds
pd.ShowHint(new Hint { Text = "Temporary!" }, duration: 5f);

// Find and modify hints
if (pd.TryGetHint("my-hint-id", out var found))
{
    found.Text = "Updated";
}

// Force update for UnSync hints
pd.ForceUpdate();
```

---

## UI Layer

### PlayerUI

> Namespace: `HintServiceMeow.UI.Utilities`

Per-player UI facade that provides access to [CommonHint](#commonhint).

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| ReferenceHub | `ReferenceHub` (readonly) | The underlying player reference |
| PlayerDisplay | `PlayerDisplay` (readonly) | The player's PlayerDisplay instance |
| CommonHint | `CommonHint` (readonly) | The common hint component |

**Static Methods:**

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| Get | `ReferenceHub referenceHub` | `PlayerUI` | Gets or creates a PlayerUI for the player |
| Get | `LabApi.Features.Wrappers.Player player` | `PlayerUI` | Gets or creates a PlayerUI (NW/LabApi) |
| Get | `Exiled.API.Features.Player player` | `PlayerUI` | Gets or creates a PlayerUI (EXILED only) |

---

### CommonHint

> Namespace: `HintServiceMeow.UI.Utilities`

Provides pre-configured hint layouts for common use cases: item descriptions, map info, role info, and general messages.

All display durations are configurable via the plugin config. The `time` parameter in each overload is in seconds.

**Methods — Item Hints:**

| Method | Parameters | Description |
|--------|------------|-------------|
| ShowItemHint | `string itemName` | Shows item name only (short duration) |
| ShowItemHint | `string itemName, float time` | Shows item name only with custom duration |
| ShowItemHint | `string itemName, string description` | Shows item name and one description line |
| ShowItemHint | `string itemName, string description, float time` | Shows item name and one description line with custom duration |
| ShowItemHint | `string itemName, string[] description` | Shows item name and multiple description lines |
| ShowItemHint | `string itemName, string[] description, float time` | Shows item name and multiple description lines with custom duration |

**Methods — Map Hints:**

| Method | Parameters | Description |
|--------|------------|-------------|
| ShowMapHint | `string roomName` | Shows room name only (short duration) |
| ShowMapHint | `string roomName, float time` | Shows room name only with custom duration |
| ShowMapHint | `string roomName, string description` | Shows room name and one description line |
| ShowMapHint | `string roomName, string description, float time` | Shows room name and one description line with custom duration |
| ShowMapHint | `string roomName, string[] description` | Shows room name and multiple description lines |
| ShowMapHint | `string roomName, string[] description, float time` | Shows room name and multiple description lines with custom duration |

**Methods — Role Hints:**

| Method | Parameters | Description |
|--------|------------|-------------|
| ShowRoleHint | `string roleName` | Shows role name only (short duration) |
| ShowRoleHint | `string roleName, float time` | Shows role name only with custom duration |
| ShowRoleHint | `string roleName, string description` | Shows role name and one description line |
| ShowRoleHint | `string roleName, string description, float time` | Shows role name and one description line with custom duration |
| ShowRoleHint | `string roleName, string[] description` | Shows role name and multiple description lines |
| ShowRoleHint | `string roleName, string[] description, float time` | Shows role name and multiple description lines with custom duration |

**Methods — Other Hints:**

| Method | Parameters | Description |
|--------|------------|-------------|
| ShowOtherHint | `string messages` | Shows a single message as a DynamicHint |
| ShowOtherHint | `string messages, float time` | Shows a single message with custom duration |
| ShowOtherHint | `string[] messages` | Shows multiple messages (duration scales with count) |
| ShowOtherHint | `string[] messages, float time` | Shows multiple messages with custom total duration |

**Usage Example:**

```csharp
var ui = PlayerUI.Get(player);
ui.CommonHint.ShowRoleHint("SCP-173", new[] { "Kill all humans", "Use your skills" });
ui.CommonHint.ShowMapHint("Heavy Containment Zone", "The place where most SCPs spawn");
ui.CommonHint.ShowItemHint("Keycard", "Used to open doors");
ui.CommonHint.ShowOtherHint("The server is starting!");
```

---

## Extension Methods

All extension methods are collected here for easy reference.

### AbstractHint Extensions

> Namespace: `HintServiceMeow.Core.Extension`

| Method | Extends | Parameters | Description |
|--------|---------|------------|-------------|
| HideAfter | `AbstractHint` | `float delay` | Sets `Hide = true` after `delay` seconds. Resets any existing hide timer |

```csharp
hint.HideAfter(5f); // Hides the hint after 5 seconds
```

### PlayerDisplay Extensions

> Namespace: `HintServiceMeow.Core.Extension`

| Method | Extends | Parameters | Description |
|--------|---------|------------|-------------|
| RemoveAfter | `PlayerDisplay` | `AbstractHint hint, float delay` | Removes the hint from the display after `delay` seconds. Resets any existing removal timer |

```csharp
playerDisplay.RemoveAfter(hint, 10f); // Removes the hint after 10 seconds
```

### NW Player Extensions

> Namespace: `HintServiceMeow.Core.Extension` / `HintServiceMeow.UI.Extension`

Extension methods for `LabApi.Features.Wrappers.Player`.

| Method | Namespace | Returns | Description |
|--------|-----------|---------|-------------|
| GetPlayerDisplay | Core | `PlayerDisplay` | Gets the player's PlayerDisplay |
| AddHint | Core | `void` | Adds a hint to the player's display |
| RemoveHint | Core | `void` | Removes a hint from the player's display |
| GetPlayerUi | UI | `PlayerUI` | Gets the player's PlayerUI instance |

```csharp
// Using NW player extensions (LabApi)
LabApi.Features.Wrappers.Player player = ...;

// Get the PlayerDisplay and add a hint directly on the player object
var hint = new Hint { Text = "Hello from NW extension!", YCoordinate = 500 };
player.AddHint(hint);

// Later, remove it
player.RemoveHint(hint);

// Access PlayerDisplay for more advanced operations
PlayerDisplay pd = player.GetPlayerDisplay();
pd.ShowHint(new Hint { Text = "Temporary!" }, duration: 3f);

// Access PlayerUI and CommonHint
PlayerUI ui = player.GetPlayerUi();
ui.CommonHint.ShowRoleHint("SCP-096", new[] { "Sit and cry", "Chase targets" });
```

### EXILED Player Extensions

> Namespace: `HintServiceMeow.Core.Extension` / `HintServiceMeow.UI.Extension`

Extension methods for `Exiled.API.Features.Player`. Only available in EXILED builds.

| Method | Namespace | Returns | Description |
|--------|-----------|---------|-------------|
| GetPlayerDisplay | Core | `PlayerDisplay` | Gets the player's PlayerDisplay |
| AddHint | Core | `void` | Adds a hint to the player's display |
| RemoveHint | Core | `void` | Removes a hint from the player's display |
| GetPlayerUi | UI | `PlayerUI` | Gets the player's PlayerUI instance |

```csharp
// Using EXILED player extensions
Exiled.API.Features.Player player = ...;

// Get the PlayerDisplay and add a hint directly on the player object
var hint = new Hint { Text = "Hello from EXILED extension!", YCoordinate = 500 };
player.AddHint(hint);

// Later, remove it
player.RemoveHint(hint);

// Access PlayerDisplay for more advanced operations
PlayerDisplay pd = player.GetPlayerDisplay();
pd.ShowHint(new Hint { Text = "Temporary!" }, duration: 3f);

// Access PlayerUI and CommonHint
PlayerUI ui = player.GetPlayerUi();
ui.CommonHint.ShowItemHint("O5 Keycard", "Grants access to all areas");
```

---

## Hint Content

These classes are used internally by hints to manage their text content. In most cases you do not need to interact with them directly — use the `Text` or `AutoText` properties on [AbstractHint](#abstracthint) instead.

### AbstractHintContent

> Namespace: `HintServiceMeow.Core.Models.HintContent`

Base class for hint content providers.

**Events:**

| Event | Type | Description |
|-------|------|-------------|
| ContentUpdated | `UpdateHandler` | Raised when the content changes |

**Methods:**

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| GetText | — | `string?` | Returns the current text content |

---

### StringContent

> Namespace: `HintServiceMeow.Core.Models.HintContent`

A content provider that holds static text. Inherits from [AbstractHintContent](#abstracthintcontent).

**Constructor:** `StringContent(string? content)`

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| Text | `string?` | The static text. Raises `ContentUpdated` when changed |

---

### AutoContent

> Namespace: `HintServiceMeow.Core.Models.HintContent`

A content provider that periodically invokes a delegate to produce dynamic text. Inherits from [AbstractHintContent](#abstracthintcontent).

**Delegate:** `delegate string TextUpdateHandler(AutoContentUpdateArg ev)`

**Constructor:** `AutoContent(TextUpdateHandler? autoText, float defaultUpdateInterval = -1)`

If `defaultUpdateInterval` is negative, defaults to `0.1` seconds.

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| AutoText | `TextUpdateHandler?` | The delegate invoked to produce text. Resetting this also resets the next update time |

**Usage Example:**

```csharp
hint.AutoText = (ev) =>
{
    ev.NextUpdateDelay = TimeSpan.FromSeconds(1); // Update every 1 second
    return $"Time: {DateTime.Now:HH:mm:ss}";
};
```

Click [here](/Docs/English/README.md) to go back to read me

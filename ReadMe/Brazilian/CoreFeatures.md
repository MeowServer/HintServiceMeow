# Documentation
This documentation introduces the usage and features of HintServiceMeow.
## Hint
Hint is a primary feature that allows you to add text to a specific position on a player's screen. The following example adds text to the lower left side of the player's screen.
```Csharp
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
Since HSM has an auto-update feature, any changes to a property (like Text, FontSize, Alignment, etc.) will automatically reflect on the player's screen without any further method calls.
```Csharp
hint.Text = "Some New Text";
// You do not have to call any method after updating properties
``` 
#### Properties
| Properties | Description |
| - | - |
| Guid (Readonly) | A generated Guid for the hint |
| Id | A custom string id |
| SyncSpeed | The priority of hint's update. The faster it is, the faster it will be updated after the content is updated |
| FontSize | The size of the text |
| LineHeight | The extra space between each line of text |
| Content | The content of the hint. The text displayed will be obtained from this property. |
| Text | Setting this property will overwrite Content with static text. |
| AutoText | Setting this property will overwrite Content with a string delegate. |
| Hide | Whether to hide this hint or not |
| XCoordinate | The horizontal position of the text. The higher the X coordinate is, the righter the text will be displayed |
| YCoordinate | The vertical position of the text. The higher the Y coordinate is, the lower the text will be displayed ![The position of Y coordinate](Images/YCoordinateExample.jpg) |
| Alignment | The alignment of the text. |
| YCoordinateAlign | The alignment of text to y coordinate. For example, Top means that the y coordinate represents the top of the text |
## DynamicHint
Dynamic hints allow you to display text without specifying a fixed position. Dynamic hints are automatically positioned on the screen in areas where they won’t overlap with other text elements, ensuring optimal readability. The following example adds text to the player's screen without setting a fixed position.
```CSharp
var dynamicHint = new DynamicHint
{
    Text = "Hello Dynamic Hint"
};

PlayerDisplay playerDisplay = PlayerDisplay.Get(player);
playerDisplay.AddHint(dynamicHint);
```
#### Properties
| Properties | Description |
| - | - |
| Guid (Readonly) | A generated Guid for the hint |
| Id | A custom string id |
| SyncSpeed | The priority of hint's update. The faster it is, the faster it will be updated after the content is updated |
| FontSize | The size of the text |
| LineHeight | The extra space between each line of text |
| Content | The content of the hint. The text displayed in Hint will be obtained from this property. |
| Text | Setting this property will overwrite content with static text. |
| AutoText | Setting this property will overwrite content with a string delegate. |
| Hide | Whether to hide this hint or not |
| TopBoundary, BottomBoundary, LeftBoundary, RightBoundary | The boundary for dynamic hint to arrange its text |
| TargetX, TargetY | The dynamic hint will try to move toward these coordinates, but the final placement depends on available space. |
| TopMargin, BottomMargin, LeftMargin, RightMargin | The extra space the hint will add to its surroundings when arranging |
| Priority | The priority of dynamic hint. The higher the priority, the former it will be arranged |
| Strategy | The strategy dynamic hint will be used when arranging |
## CommonHint
CommonHint is a component that allows you to display text in a preset position. The following example uses CommonHint to display multiple messages to the player.
```CSharp
var ui = PlayerUI.Get(player);
ui.CommonHint.ShowRoleHint("SCP173", new[] { "Kill all humans", "Use your skills" });
ui.CommonHint.ShowMapHint("Heavy Containment Zone", "The place where most SCPs spawn");
ui.CommonHint.ShowItemHint("Keycard", "Used to open doors");
ui.CommonHint.ShowOtherHint("The server is starting!");
```

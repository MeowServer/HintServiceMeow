Click [here](/Docs/English/README.md) to go back to read me

## Getting Started
### Set up dependencies
1. Create your C# project
2. Include the dll file downloaded from release into your project's dependencies
### Show your first hint
The following code blocks show how to use frequently used features of HSM.

---
Create a Hello World hint on player's screen.
```CSharp
Player player = Player.Get(xxx);

// Hint is a simple hint that can be shown on the player's screen.
Hint hint1 = new Hint
{
    Text = "Hello World" // You can set the properties of the hint in a pair of braces({})
};

// You can set the properties of the hint like this:
hint1.FontSize = 40;
hint1.YCoordinate = 700;
hint1.Alignment = HintAlignment.Left;
// After setting the properties, you don't need to use method to call for an update. All update will be done automatically by the HSM(HintServiceMeow).

// You can show a hint to a player by adding it to the player's PlayerDisplay.
// You can also remove it by removing it from the PlayerDisplay.
PlayerDisplay playerDisplay = PlayerDisplay.Get(player);
playerDisplay.AddHint(hint1);
// playerDisplay.RemoveHint(hint);

```
---
Use `AutoText` to create a hint that update the content automatically.

Use extension to add or remove hint in a simple manner.
```CSharp
Hint hint2 = new Hint
{
    AutoText = ev => DateTime.Now.ToString("HH:mm:ss"), // You can also use a function to set the text of the hint, and the hint will update itself.
    Alignment = HintAlignment.Right, // You can set the properties of the hint in any order you want, and you can also choose to not set some properties, because all properties have a default value.
    YCoordinate = 200
};

// You can also use extension method to make it easier
player.AddHint(hint2); // This is equivalent to playerDisplay.AddHint(hint);
// player.RemoveHint(hint); // This is equivalent to playerDisplay.RemoveHint(hint);

```
---
Use `NextUpdateDelay` to set the custom update rate of your AutoText.

Use `PlayerDisplay::ShowHint(Hint, float)` to show a hint for a certain time.
```CSharp
Hint hint3 = new Hint()
{
    YCoordinate = 300,
    Alignment = HintAlignment.Right,
    AutoText = ev =>
    {
        ev.NextUpdateDelay = TimeSpan.FromSeconds(2f); // You can set the next update delay in the event argument, and the hint will update itself after the delay. This is useful when you want to update the hint after a certain time interval.

        return "TPS: " + Server.Tps.ToString("F2");
    },
};

// If you only want to show a hint temporarily, you can use ShowHint
playerDisplay.ShowHint(hint3, 12f); // This shows a hint for 12 seconds, then hide it.

```
---
Use Dynamic Hint to help avoid conflict.
```CSharp
// DynamicHint is a hint that can automatically arrange itself to avoid overlapping with other hints.
DynamicHint dynamicHint = new DynamicHint
{
    Text = "Hello Dynamic Hint",
    TargetX = 100f,
};

playerDisplay.AddHint(dynamicHint);

```
---
Use CommonHint to quickly develop your UI.
```CSharp
// PlayerUI::CommonHint is a set of preset hints that help you to show hints easily
PlayerUI ui = PlayerUI.Get(player);
ui.CommonHint.ShowRoleHint("SCP173", ["Kill all humans", "Use your skills"]);
ui.CommonHint.ShowMapHint("Heavy Containment Zone", "The place where most SCPs spawn");
ui.CommonHint.ShowItemHint("Keycard", "Used to open doors");
ui.CommonHint.ShowOtherHint("The server is starting!");
```
---
Use `ResolutionOption` to adapt hints for different screen resolutions.
```CSharp
// ResolutionOption.Offset automatically pushes left/right aligned hints toward the screen edge based on the player's screen resolution.
Hint resolutionHint = new Hint
{
    Text = "Adapted to screen edge",
    Alignment = HintAlignment.Left,
    YCoordinate = 400,
    ResolutionOption = ResolutionOption.Offset // This is the default value. Set to ResolutionOption.None to disable.
};

playerDisplay.AddHint(resolutionHint);

```
---
Use `RichTag` helpers to easily apply Unity rich text tags.
```CSharp
// Use the / operator or UseTag() extension to wrap text with rich text tags
string colored = "Hello" / ColorTag.Red; // Result: <color=#FF0000>Hello</color>
string bold = "World".UseTag(BoldTag.Bold); // Result: <b>World</b>
string styled = "Fancy".UseTag(ColorTag.Get("#FF8800"), BoldTag.Bold, SizeTag.Get(30)); // Combine multiple tags

Hint richHint = new Hint
{
    Text = colored + " " + bold + "\n" + styled,
    YCoordinate = 500,
};

playerDisplay.AddHint(richHint);

```
---
Use `Transition` to animate hint property changes.
```CSharp
// Transition smoothly animates property changes over a duration
Hint animatedHint = new Hint
{
    Text = "I move smoothly!",
    YCoordinate = 600,
    FontSize = 20,
    YCoordinateTransition = Transition.Get(EasingType.EaseInOut, duration: 1f), // Animate Y position changes over 1 second
    FontSizeTransition = Transition.Get(EasingType.EaseOut, duration: 0.5f), // Animate font size changes over 0.5 seconds
};

playerDisplay.AddHint(animatedHint);

// When you change the property, the transition animates it smoothly
animatedHint.YCoordinate = 200; // Smoothly moves from 600 to 200 over 1 second
animatedHint.FontSize = 40; // Smoothly grows from 20 to 40 over 0.5 seconds

```
---
Use `HintTemplate` to quickly create hints with preset properties.
```CSharp
// HintTemplate is a blueprint for creating hints with predefined properties
HintTemplate template = new HintTemplate
{
    FontSize = 25,
    YCoordinate = 700,
    Alignment = HintAlignment.Right,
    FontSizeTransition = Transition.Get(EasingType.EaseInOut, 0.5f),
};

// Use GetHint() to create a new hint from the template
Hint hintFromTemplate = template.GetHint();
hintFromTemplate.Text = "Created from template";
playerDisplay.AddHint(hintFromTemplate);

// Or use Apply() to apply the template to an existing hint
Hint existingHint = new Hint { Text = "Existing hint" };
template.Apply(existingHint); // Applies FontSize, YCoordinate, Alignment, and FontSizeTransition
playerDisplay.AddHint(existingHint);

```
---
The above code blocks will create an UI like this:
![Hint view](Images/GettingStartedExample.jpg)
Labeled:
![Hint view labeled](Images/GettingStartedExampleLabeled.jpg)

Read [Core Features](CoreFeatures.md) to learn more

Click [here](/Docs/English/README.md) to go back to read me
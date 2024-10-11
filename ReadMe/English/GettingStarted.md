## Getting Started
### Set up dependencies
1. Create your C# project
2. Include the dll file downloaded from release into your project's dependecies
### Show your first hint
The following code show "Hello World" on the left bottom side of player's screen.
```CSharp
//Bind this to player verified(player joined) event
public static void OnVerified(VerifiedEventArgs ev)
{
    //Create a hint instance
    Hint hint = new Hint
    {
        Text = "Hello World"
    };

    //Set hint's properties(optional)
    hint.YCoordinate = 200;
    hint.Alignment = HintAlignment.Left;

    //Get player display, and add hint into player display
    PlayerDisplay playerDisplay = PlayerDisplay.Get(ev.Player);
    playerDisplay.AddHint(hint);
}
```
![The hint view](Images/GettingStartedExample.jpg)
# HintServiceMeow Integrations
## Scripted Event
Script event command:
    Name: "HSM_CommonHint";
    Aliases: "HSM_CH" 
    Subgroup: Broadcast;
    Arguments:
    - Players (Required)
        A player variable, the player to show the hint. 
    - Text (Required) 
        A string, the text of the common hint. You can use [End-Line] tag to start a new line. The maximum num of line is 4. 
    - Duration (Not Required)
        A number, the duration of the common hint. 
## 开始使用
### 设置依赖
1. 创建你的C#项目
2. 将Release中的dll文件添加到项目的依赖中
### 显示第一个Hint
以下代码在玩家的屏幕的左下角显示了"Hello World"
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
    hint.YCoordinate = 700;
    hint.Alignment = HintAlignment.Left;

    //Get player display, and add hint into player display
    PlayerDisplay playerDisplay = PlayerDisplay.Get(ev.Player);
    playerDisplay.AddHint(hint);
}
```
![The hint view](Images/GettingStartedExample.jpg)
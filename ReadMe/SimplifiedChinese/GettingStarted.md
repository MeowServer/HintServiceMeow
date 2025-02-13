## 开始使用
### 设置依赖
1. 创建你的C#项目
2. 将Release中的dll文件添加到项目的依赖中
### 显示第一个Hint
以下代码在玩家的屏幕的左下角显示了"Hello World"
```CSharp
//将此方法和玩家加入服务器事件绑定
public static void OnVerified(VerifiedEventArgs ev)
{
    //创建一个Hint类的实例
    Hint hint = new Hint
    {
        Text = "Hello World"
    };

    //设置Hint的属性
    hint.YCoordinate = 700;
    hint.Alignment = HintAlignment.Left;

    //通过PlayerDisplay.Get()获取PlayerDisplay，然后将Hint添加到PlayerDisplay中
    PlayerDisplay playerDisplay = PlayerDisplay.Get(ev.Player);
    playerDisplay.AddHint(hint);
}
```
![The hint view](Images/GettingStartedExample.jpg)

# CheatConsole
Unity CheatConsole Plugin
this is a simple tutorial to help you integrate CheatConsole Project to your project.
1. this project can work with unity 4.0.0 or later.
2. you must create a gameobj which is named 'RootObj' in your BootScene. and then the MonoSingleton class can work well.
3. you can add the GameFramework Component to any objects in BootScene, or you can add this initialize code to your startup codes:
	
#if !WITH_OUT_CHEAT_CONSOLE
            ConsoleWindow.instance.isVisible = false;
#endif
	this codes will create a ConsoleWindow and hook the F1 press.
4. press F1, it will display pc mode cheat console.
   press Ctrl+F1, it will display mobile mode cheat console.
   in editor, there is a editor mode cheat console panel in the tools window.
   for more detail, you can see : https://www.youtube.com/watch?v=g1b-a_XpxAc
5. you can add macro WITH_OUT_CHEAT_CONSOLE to player settings to disable all console codes.
6. There are two main ways to implement cheat commands: 1. Customize a class derived from CheatCommandCommon, and specify how to recognize this command through Attributes such as ArgumentDescription. The execution of the instruction is realized by overriding the Execute method. This method is suitable for some complex instructions.(see Tests/CommandsTests.cs) 2. Simple mode, you just need to find a random class, implement a static function, and use CheatCommandEntryMethodAttribute to mark this function as a cheat command.(see Tests/CommandTests2.cs TestCommandEntry).

tips:
  F1 open pc mode
  Ctrl+F1 open mobile mode
  in pc mode:
     up arrow/down arrow, select in candinate lists
	 ctrl + up arrow/down arrow, select in history lists.

email:
    dbdongbo@vip.qq.com

----
1.首先你需要在你的启动关卡里面放置一个RootObj的GameObject，因为MonoSingleton这个类依赖于它。
2.其次你需要挂一个叫GameFramework的MonoBehaviour到启动场景中的任意GameObject身上。
3.启动后，你可以使用F1或者Ctrl+F1来打开面板。也可以通过Tools/Cheat Panel来打开编辑器模式的面板，它们用的是同一套代码。

这里主要有两种方法实现作弊指令：
1.完全模式： 自定义从CheatCommandCommon派生的类，通过ArgumentDescription等Attribute来指定如何识别这个指令。通过重写Execute方法来实现指令的执行。这种方法适用于一些复杂的指令。
2.简单模式： 你只需要随便找个类，实现一个静态函数，并使用CheatCommandEntryMethodAttribute将这个函数标记成一个作弊指令即可。当然，如果这个函数中的参数之类系统不支持的话，将不会被识别成作弊指令。
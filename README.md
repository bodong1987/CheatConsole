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

tips:
  F1 open pc mode
  Ctrl+F1 open mobile mode
  in pc mode:
     up arrow/down arrow, select in candinate lists
	 ctrl + up arrow/down arrow, select in history lists.

email:
    bodong@tencent.com or
    dbdongbo@vip.qq.com

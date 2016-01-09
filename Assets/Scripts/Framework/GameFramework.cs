using Assets.Scripts.Common;

#if !WITH_OUT_CHEAT_CONSOLE
using Assets.Scripts.Console;
#endif


namespace Assets.Scripts.Framework
{
    public class GameFramework :
        MonoSingleton<GameFramework>
    {
        protected override void Init()
        {
            base.Init();

#if !WITH_OUT_CHEAT_CONSOLE
            ConsoleWindow.instance.isVisible = false;
            
            // you can set this flag by the networking message
            // eg:is this account a gm account?
            ConsoleWindow.instance.bEnableCheatConsole = true;

            CheatCommandRegister.instance.Register(typeof(GameFramework).Assembly);
#endif
        }
    }
}
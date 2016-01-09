using Assets.Scripts.Common;

#if !WITH_OUT_CHEAT_CONSOLE
using Assets.Scripts.Console;
#endif


namespace Assets.Scripts.Framework
{
    public class GameFramework :
        MonoSingleton<GameFramework>
    {
        // you can set this flag by the networking message
        // eg:is this account a gm account?
        public bool bSupportCheatConsole = true;

        protected override void Init()
        {
            base.Init();

#if !WITH_OUT_CHEAT_CONSOLE           
            if(bSupportCheatConsole)
            {
                CheatCommandRegister.instance.Register(typeof(GameFramework).Assembly);
                ConsoleWindow.instance.isVisible = false;

                ConsoleWindow.instance.bEnableCheatConsole = bSupportCheatConsole;
            }
#endif
        }
    }
}
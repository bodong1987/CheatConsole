/**
 * @brief Basic Demo Commands
 * @email bodong@tencent.com
*/

#if !WITH_OUT_CHEAT_CONSOLE
// 
namespace Assets.Scripts.Console
{
    [CheatCommandEntryAttribute("Tools")]
    class CheatCommandCommonEntry
    {
        [CheatCommandEntryMethodAttribute("Exit", false)]
        public static string Exit()
        {
            if (ConsoleWindow.HasInstance())
            {
                ConsoleWindow.instance.isVisible = false;
            }

            return CheatCommandBase.Done;
        }

        [CheatCommandEntryMethodAttribute("Quit", false)]
        public static string Quit()
        {
            if (ConsoleWindow.HasInstance())
            {
                ConsoleWindow.instance.isVisible = false;
            }

            return CheatCommandBase.Done;
        }

        [CheatCommandEntryMethodAttribute("Clean Screen", false)]
        public static string Clean()
        {
            if (ConsoleWindow.HasInstance())
            {
                ConsoleWindow.instance.ClearLog();
            }

            return CheatCommandBase.Done;
        }

        public enum EConsoleView
        {
            PC,
            Mobile
        }

        [CheatCommandEntryMethodAttribute("Toggle Console View", false)]
        public static string ToggleView(EConsoleView InView)
        {
            if (ConsoleWindow.HasInstance())
            {
                if( InView == EConsoleView.PC )
                {
                    ConsoleWindow.instance.ChangeToPCView();
                }
                else if( InView == EConsoleView.Mobile )
                {
                    ConsoleWindow.instance.ChangeToMobileView();
                }                
            }

            return CheatCommandBase.Done;
        }
    }
}
#endif
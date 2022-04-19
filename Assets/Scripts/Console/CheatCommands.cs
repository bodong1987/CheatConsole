/**
 * @brief Basic Demo Commands
 * @email dbdongbo@vip.qq.com
*/
#if !WITH_OUT_CHEAT_CONSOLE
using System;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Common;
using System.Linq;


namespace Assets.Scripts.Console
{  
    enum ListCommandTargetType
    {
        Group,
        Command
    }

    [CheatCommandAttribute("Tools/LS", "Display Commands")]
    [ArgumentDescriptionAttribute(0, typeof(ListCommandTargetType), "TargetType")]
    class ListCommand : CheatCommandCommon
    {
        protected override string Execute(string[] InArguments)
        {
            if (ConsoleWindow.HasInstance())
            {
                ListCommandTargetType TargetType = (ListCommandTargetType)StringToEnum(InArguments[0], typeof(ListCommandTargetType));

                if (TargetType == ListCommandTargetType.Group)
                {
                    var Repositories = CheatCommandsRepository.instance.repositories;
                    var Iter = Repositories.GetEnumerator();

                    while (Iter.MoveNext())
                    {
                        ConsoleWindow.instance.AddMessage(Iter.Current.Key);
                    }
                }
                else if (TargetType == ListCommandTargetType.Command)
                {
                    var AllCommands = CheatCommandsRepository.instance.generalRepositories.Commands;
                    var Iter = AllCommands.GetEnumerator();

                    while (Iter.MoveNext())
                    {
                        ConsoleWindow.instance.AddMessage(Iter.Current.Value.fullyHelper);
                    }
                }
                else
                {
                    DebugHelper.Assert(false, "internal error!");
                }

                return Done;
            }

            return "Without Console Window Instance.";
        }
    }
}
#endif
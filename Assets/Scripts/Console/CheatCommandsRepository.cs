/**
 * @brief CheatCommand Storage
 * @email bodong@tencent.com
*/

#if !WITH_OUT_CHEAT_CONSOLE
using Assets.Scripts.Common;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Console
{

    public class CheatCommandGroup
    {
        public DictionaryView<string, ICheatCommand> Commands = new DictionaryView<string, ICheatCommand>();
        public DictionaryView<string, CheatCommandGroup> ChildrenGroups = new DictionaryView<string, CheatCommandGroup>();

#if UNITY_EDITOR
        public int Selection = 0;
#endif

        public void AddCommand(ICheatCommand InCommand, int HierarchiesIndex)
        {
            DebugHelper.Assert(InCommand != null);
            string[] GroupHierarchies = InCommand.command.groupHierarchies;

            DebugHelper.Assert(GroupHierarchies != null);

            if (HierarchiesIndex < GroupHierarchies.Length)
            {
                // should add to children group
                CheatCommandGroup Groups = null;
                if (!ChildrenGroups.TryGetValue(GroupHierarchies[HierarchiesIndex], out Groups))
                {
                    Groups = new CheatCommandGroup();
                    ChildrenGroups.Add(GroupHierarchies[HierarchiesIndex], Groups);
                }

                DebugHelper.Assert(Groups != null);

                Groups.AddCommand(InCommand, HierarchiesIndex + 1);
            }
            else
            {
                // add to this group 
                Commands.Add(InCommand.command.baseName, InCommand);
            }
        }
    }

    public class CheatCommandsRepository : Singleton<CheatCommandsRepository>
    {
        private CheatCommandGroup GeneralRepositories = new CheatCommandGroup();
        private DictionaryView<string, CheatCommandGroup> Repositories = new DictionaryView<string, CheatCommandGroup>();

        public DictionaryView<string, CheatCommandGroup> repositories { get { return Repositories; } }
        public CheatCommandGroup generalRepositories { get { return GeneralRepositories; } }

        public void RegisterCommand(ICheatCommand InCommand)
        {
            // 1.
            DebugHelper.Assert(InCommand != null && !HasCommand(InCommand.command.baseName));

            GeneralRepositories.Commands[InCommand.command.baseName.ToLower()] = InCommand;

            // 2.
            string[] GroupHierarchies = InCommand.command.groupHierarchies;

            DebugHelper.Assert(GroupHierarchies != null);

            string BaseGroup = GroupHierarchies[0];

            CheatCommandGroup Groups = null;
            if (!Repositories.TryGetValue(BaseGroup, out Groups))
            {
                Groups = new CheatCommandGroup();
                Repositories[BaseGroup] = Groups;
            }

            Groups.AddCommand(InCommand, 1);
        }

        public bool HasCommand(string InCommand)
        {
            return GeneralRepositories.Commands.ContainsKey(InCommand.ToLower());
        }

        public ICheatCommand FindCommand(string InCommand)
        {
            ICheatCommand Result = null;

            GeneralRepositories.Commands.TryGetValue(InCommand.ToLower(), out Result);

            return Result;
        }

        public string ExecuteCommand(string InCommand, string[] InArgs)
        {
            if (HasCommand(InCommand))
            {
                return GeneralRepositories.Commands[InCommand.ToLower()].StartProcess(InArgs);
            }
            else
            {
                return "Command not found";
            }
        }

        public ListView<ICheatCommand> FilterByString(string InPrefix)
        {
            DebugHelper.Assert(InPrefix != null);

            ListView<ICheatCommand> Results = new ListView<ICheatCommand>(16);

            var Iter = GeneralRepositories.Commands.GetEnumerator();

            while (Iter.MoveNext())
            {
                var Command = Iter.Current.Value;

                if (Command.command.baseName.StartsWith(InPrefix, StringComparison.CurrentCultureIgnoreCase) ||
                    string.IsNullOrEmpty(InPrefix)
                    )
                {
                    Results.Add(Command);
                }
            }

            return Results;
        }
    }
}
#endif
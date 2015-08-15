/**
 * @brief Automantic Draw In Editor
 * @email bodong@tencent.com
*/
#if !WITH_OUT_CHEAT_CONSOLE && UNITY_EDITOR

using Assets.Scripts.Common;
using Assets.Scripts.Console;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Console
{

    public class CheatConsolePanel : EditorWindow
    {
        private Vector2 ScrollPosition;
        private static IConsoleLogger Logger = new MobileLogger();

        [MenuItem("Tools/CheatPanel")]
        public static void ShowCheatWindow()
        {
            CheatCommandRegister.GetInstance();

            GetWindowSelf();

            ConsoleWindow.instance.externalLogger = Logger;
        }

        static CheatConsolePanel GetWindowSelf()
        {
            return GetWindow<CheatConsolePanel>("Cheat Panel");
        }

        static void UpdatePanel()
        {
            CheatConsolePanel Inst = GetWindowSelf();

            if (Inst != null)
            {
                Inst.Repaint();
            }
        }

        virtual protected void OnGUI()
        {
            // display error message        
            GUI.color = Color.yellow;
            EditorGUILayout.LabelField(Logger.message, EditorStyles.boldLabel);
            GUI.color = Color.white;

            ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);
            if (!Application.isPlaying)
            {
                EditorGUI.BeginDisabledGroup(true);
            }

            DrawFromCheatCommandRepository();

            if (!Application.isPlaying)
            {
                EditorGUI.BeginDisabledGroup(false);
            }

            EditorGUILayout.EndScrollView();
        }

        List<string> ValidGroups = new List<string>();
        string[] ToolbarTitles = null;
        int CurrentSelection = 0;

        // top view
        private void DrawFromCheatCommandRepository()
        {
            var Repositories = CheatCommandsRepository.instance.repositories;

            DebugHelper.Assert(Repositories != null);

            if (ValidGroups.Count <= 0)
            {
                var Iter = Repositories.GetEnumerator();

                while (Iter.MoveNext())
                {
                    // string GroupName = Iter.Current.Key;
                    var Commands = Iter.Current.Value.Commands;

                    if (Iter.Current.Value.ChildrenGroups.Count > 0 || HasValidCommand(Commands))
                    {
                        ValidGroups.Add(Iter.Current.Key);
                    }
                }

                ToolbarTitles = ValidGroups.ToArray();
            }

            if (ToolbarTitles != null)
            {
                CurrentSelection = GUILayout.Toolbar(CurrentSelection, ToolbarTitles);
            }

            if (CurrentSelection >= 0 && ToolbarTitles != null && CurrentSelection < ToolbarTitles.Length)
            {
                var Iter = Repositories.GetEnumerator();

                while (Iter.MoveNext())
                {
                    string GroupName = Iter.Current.Key;
                    // var Commands = Iter.Current.Value.Commands;

                    if (ToolbarTitles[CurrentSelection] == GroupName)
                    {
                        GUILayout.BeginVertical();

                        DrawCommandsGroup(GroupName, Iter.Current.Value);

                        GUILayout.EndVertical();

                        DrawSplitter();

                        break;
                    }
                }
            }
        }

        private bool HasValidCommand(DictionaryView<string, ICheatCommand> InCommands)
        {
            var iter = InCommands.GetEnumerator();

            while (iter.MoveNext())
            {
                if (iter.Current.Value.isSupportInEditor)
                {
                    return true;
                }
            }

            return false;
        }

        private void DrawCommandsGroup(string InGroupName, CheatCommandGroup InGroups)
        {
            {
                var iter = InGroups.Commands.GetEnumerator();

                while (iter.MoveNext())
                {
                    if (iter.Current.Value.isSupportInEditor)
                    {
                        DrawCommand(InGroupName, iter.Current.Key, iter.Current.Value);
                    }
                }
            }

            {
                if (InGroups.ChildrenGroups.Count > 0)
                {
                    string[] LocalToolbarTitles = new string[InGroups.ChildrenGroups.Count];

                    {
                        var Iter = InGroups.ChildrenGroups.GetEnumerator();

                        int Index = 0;
                        while (Iter.MoveNext())
                        {
                            LocalToolbarTitles[Index] = Iter.Current.Key;

                            ++Index;
                        }

                        InGroups.Selection = GUILayout.Toolbar(InGroups.Selection, LocalToolbarTitles);
                    }


                    if (InGroups.Selection >= 0 && LocalToolbarTitles != null && InGroups.Selection < LocalToolbarTitles.Length)
                    {
                        var Iter = InGroups.ChildrenGroups.GetEnumerator();

                        while (Iter.MoveNext())
                        {
                            string GroupName = Iter.Current.Key;
                            //  var Commands = Iter.Current.Value.Commands;

                            if (LocalToolbarTitles[InGroups.Selection] == GroupName)
                            {
                                GUILayout.BeginVertical();

                                DrawCommandsGroup(GroupName, Iter.Current.Value);

                                GUILayout.EndVertical();

                                DrawSplitter();

                                break;
                            }
                        }
                    }
                }
            }
        }

        private void DrawSplitter()
        {
            GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(1));
        }

        private void DrawCommand(string InGroupName, string InName, ICheatCommand InCommand)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            ArgumentDescriptionAttribute[] ArgTypes = InCommand.argumentsTypes;
            string[] Arguments = InCommand.arguments;

            if (ArgTypes != null && ArgTypes.Length > 0)
            {
                DebugHelper.Assert(ArgTypes.Length == Arguments.Length);

                for (int i = 0; i < ArgTypes.Length; ++i)
                {
                    ArgumentDescriptionAttribute ArgAttr = ArgTypes[i];

                    if (!DrawArgument(ArgAttr, ArgTypes, ref Arguments, ref Arguments[i]))
                    {
                        break;
                    }
                }
            }

            if (GUILayout.Button(InCommand.comment, GUILayout.Width(InCommand.comment.Length * 12 + 10)))
            {
                Logger.AddMessage(InCommand.StartProcess(Arguments));
            }

            GUILayout.EndHorizontal();
        }

        private bool ShouldSkip(ArgumentDescriptionAttribute InArgAttr, ref string[] ExistsValues)
        {
            DebugHelper.Assert(InArgAttr.isOptional);

            DependencyDescription[] Dependencies = InArgAttr.depends;

            for (int i = 0; i < Dependencies.Length; ++i)
            {
                DependencyDescription Depend = Dependencies[i];

                string ExistsValue = ExistsValues[Depend.dependsIndex];

                if (Depend.ShouldBackOff(ExistsValue))
                {
                    return false;
                }
            }

            return true;
        }

        private bool DrawArgument(
            ArgumentDescriptionAttribute InArgAttr,
            ArgumentDescriptionAttribute[] InArgTypes,
            ref string[] OutValues,
            ref string OutValue
        )
        {
            string DummyString = "";

            if (InArgAttr.isOptional && ShouldSkip(InArgAttr, ref OutValues))
            {
                return false;
            }

            EditorGUILayout.LabelField(InArgAttr.name, GUILayout.Width(InArgAttr.name.Length * 12 + 5));

            if (InArgAttr.isEnum)
            {
                Enum TagEnum = null;

                if (string.IsNullOrEmpty(OutValue))
                {
                    Array Values = Enum.GetValues(InArgAttr.argumentType);

                    DebugHelper.Assert(Values.Length > 0);

                    TagEnum = (Enum)Values.GetValue(0);
                }
                else
                {
                    TagEnum = (Enum)Enum.Parse(InArgAttr.argumentType, OutValue);
                }

                Enum Result = EditorGUILayout.EnumPopup(TagEnum, GUILayout.Width(60));

                OutValue = Result.ToString();
            }
            else if (InArgAttr.argumentType == typeof(int))
            {
                int Value = 1;

                if (CheatCommandCommon.TypeCastCheck(OutValue, typeof(int), out DummyString))
                {
                    Value = CheatCommandCommon.SmartConvert<int>(OutValue);
                }

                Value = EditorGUILayout.IntField(Value, GUILayout.Width(60));

                OutValue = Value.ToString();
            }
            else if (InArgAttr.argumentType == typeof(float))
            {
                float Value = 1;

                if (CheatCommandCommon.TypeCastCheck(OutValue, typeof(float), out DummyString))
                {
                    Value = CheatCommandCommon.SmartConvert<float>(OutValue);
                }

                Value = EditorGUILayout.FloatField(Value, GUILayout.Width(60));

                OutValue = Value.ToString();
            }
            else
            {
                // string
                OutValue = EditorGUILayout.TextField(OutValue, GUILayout.Width(80));
            }

            return true;
        }
    }
}

#endif

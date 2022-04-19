﻿/**
 * @brief Automantic Mobile View Drawer
 * @email dbdongbo@vip.qq.com
*/

#if !WITH_OUT_CHEAT_CONSOLE
using Assets.Scripts.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Console
{
    public class MobileLogger : IConsoleLogger
    {
        string Message = "";

        public string message { get { return Message; } }

        // add new log message
        public void AddMessage(string InMessage)
        {
            Message = InMessage;
        }

        // clear all log text
        public void Clear()
        {
            Message = "";
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    class CommandDisplayAttribute : AutoRegisterAttribute
    {
    }

    abstract class CommandDisplayBasicState : BaseState
    {
        protected ConsoleWindow ParentWindow;
        protected ConsoleViewMobile ParentView;

#if UNITY_STANDALONE || UNITY_EDITOR
        public static int SpaceHeight = 1;
#elif UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8
    public static int SpaceHeight = 3;
#endif


        public CommandDisplayBasicState(ConsoleWindow InParentWindow, ConsoleViewMobile InParentView)
        {
            ParentWindow = InParentWindow;
            ParentView = InParentView;
        }

        public IConsoleLogger logger
        {
            get
            {
                return ParentView.logger;
            }
        }

        public abstract void OnGUI();

        public bool DrawButton(string InButtonText, string InToolTip = "")
        {
            var Content = new GUIContent(InButtonText, InToolTip);
            var Size = ParentView.CustomButtonStyle.CalcSize(Content);

            return GUILayout.Button(Content, ParentView.CustomButtonStyle, GUILayout.Width(Size.x + 20));
        }

        public void DrawLabel(string InLabel)
        {
            var Content = new GUIContent(InLabel);
            var Size = ParentView.CustomLabelStyle.CalcSize(Content);

            GUILayout.Label(Content, ParentView.CustomLabelStyle, GUILayout.Width(Size.x + 15));
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            OnResetSkipCount();
        }

        public override void OnStateResume()
        {
            base.OnStateResume();

            OnResetSkipCount();
        }

        protected virtual void OnResetSkipCount()
        {

        }

        public virtual bool canScroll
        {
            get
            {
                return true;
            }
        }
    }

    [CommandDisplayAttribute]
    class CommandGroupDisplayState : CommandDisplayBasicState
    {
        public CommandGroupDisplayState(ConsoleWindow InParentWindow, ConsoleViewMobile InParentView) :
            base(InParentWindow, InParentView)
        {
        }

        public override void OnGUI()
        {
            GUI.contentColor = Color.green;

            try
            {
                var Repositories = CheatCommandsRepository.instance.repositories;

                // Draw All Groups on Screen
                DebugHelper.Assert(Repositories != null);

                var Iter = Repositories.GetEnumerator();

                int Index = 0;

                int Count = 0;

                while (Iter.MoveNext())
                {
                    string GroupName = Iter.Current.Key;

                    if (Count++ >= ParentView.skipCount && DrawButton(GroupName))
                    {
                        ParentView.SelectGroup(Iter.Current.Value);
                        break;
                    }

                    GUILayout.Space(SpaceHeight);

                    ++Index;
                }
            }
            finally
            {
                GUI.contentColor = Color.white;
            }
        }

        protected override void OnResetSkipCount()
        {
            ParentView.UpdateSkipCount(CheatCommandsRepository.instance.repositories.Count);
        }               

        public override bool canScroll
        {
            get
            {
                return true;
            }
        }
    }

    [CommandDisplayAttribute]
    class CommandInGroupDisplayState : CommandDisplayBasicState
    {
        public CheatCommandGroup currentGroup { get; protected set; }
        int Count = 0;

        public CommandInGroupDisplayState(ConsoleWindow InParentWindow, ConsoleViewMobile InParentView) :
            base(InParentWindow, InParentView)
        {
        }

        public void SetGroup(CheatCommandGroup InGroup)
        {
            currentGroup = InGroup;

            OnResetSkipCount();
        }

        protected override void OnResetSkipCount()
        {
            if(currentGroup != null)
            {
                int CommandsCount = 0;

                var Iter = currentGroup.Commands.GetEnumerator();

                while (Iter.MoveNext())
                {
                    if (!Iter.Current.Value.isHiddenInMobile)
                    {
                        ++CommandsCount;
                    }
                }

                ParentView.UpdateSkipCount(currentGroup.ChildrenGroups.Count + CommandsCount);
            }
        }

        protected void DrawGroups()
        {
            var Iter = currentGroup.ChildrenGroups.GetEnumerator();

            while (Iter.MoveNext())
            {
                string GroupName = Iter.Current.Key;

                if (Count++ >= ParentView.skipCount && DrawButton(GroupName))
                {
                    ParentView.SelectGroup(Iter.Current.Value);
                }

                GUILayout.Space(SpaceHeight);
            }
        }

        protected void DrawCommands()
        {
            var Iter = currentGroup.Commands.GetEnumerator();

            int Index = 0;

            while (Iter.MoveNext())
            {
                if (Iter.Current.Value.isHiddenInMobile)
                {
                    continue;
                }

                GUILayout.BeginHorizontal();

                try
                {
                    string FunctionName = Iter.Current.Value.comment;

                    if (Count++ >= ParentView.skipCount)
                    {
                        if (DrawButton(FunctionName, Iter.Current.Value.fullyHelper))
                        {
                            ICheatCommand Command = Iter.Current.Value;

                            if (Command.argumentsTypes == null ||
                                Command.argumentsTypes.Length == 0)
                            {
                                // execute this command directly.
                                logger.AddMessage(Command.StartProcess(new string[] { "" }));
                            }
                            else
                            {
                                logger.Clear();
                                ParentView.SelectionCommand(Iter.Current.Value);
                                break;
                            }
                        }

                        GUILayout.Label(GUI.tooltip, ParentView.CustomLabelStyle);

                        GUI.tooltip = "";
                    }

                    ++Index;
                }
                finally
                {
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(SpaceHeight);
            }
        }

        public override void OnGUI()
        {
            if (currentGroup == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(logger.message))
            {
                GUI.contentColor = Color.yellow;
                GUILayout.Label(logger.message, ParentView.CustomLabelStyle);
            }

            // draw all groups first
            Count = 0;

            GUI.contentColor = Color.green;
            try
            {
                DrawGroups();
            }
            finally
            {
                GUI.contentColor = Color.white;
            }

            DrawCommands();
        }
    }

    [CommandDisplayAttribute]
    class CommandDisplayState : CommandDisplayBasicState
    {
        ICheatCommand CheatCommand = null;

        public CommandDisplayState(ConsoleWindow InParentWindow, ConsoleViewMobile InParentView) :
            base(InParentWindow, InParentView)
        {
        }

        public void ResetCheatCommand(ICheatCommand InCommand)
        {
            CheatCommand = InCommand;
        }

        public override void OnGUI()
        {
            if (CheatCommand == null)
            {
                return;
            }

            GUI.contentColor = Color.green;
            GUILayout.Label(CheatCommand.fullyHelper, ParentView.CustomSmallLabelStyle);

            GUI.contentColor = Color.yellow;
            GUILayout.Label(logger.message, ParentView.CustomSmallLabelStyle);
            GUI.contentColor = Color.white;

            DrawArugments();
        }

        protected void DrawArugments()
        {
            ArgumentDescriptionAttribute[] ArgTypes = CheatCommand.argumentsTypes;
            string[] Arguments = CheatCommand.arguments;

            int DrawCount = 0;

            if (ArgTypes != null && ArgTypes.Length > 0)
            {
                DebugHelper.Assert(ArgTypes.Length == Arguments.Length);

                for (int i = 0; i < ArgTypes.Length; ++i)
                {
                    ArgumentDescriptionAttribute ArgAttr = ArgTypes[i];

                    if (!DrawArgument(ArgAttr, i, ArgTypes, ref Arguments, ref Arguments[i]))
                    {
                        break;
                    }

                    ++DrawCount;
                }
            }

            if (DrawButton(CheatCommand.comment))
            {
                logger.AddMessage(CheatCommand.StartProcess(Arguments));
            }
        }

        private bool DrawArgument(
            ArgumentDescriptionAttribute InArgAttr,
            int InIndex,
            ArgumentDescriptionAttribute[] InArgTypes,
            ref string[] OutValues,
            ref string OutValue
        )
        {
            if (InArgAttr.isOptional && ShouldSkip(InArgAttr, ref OutValues))
            {
                return false;
            }

            GUILayout.BeginHorizontal();

            DrawLabel(InArgAttr.name);

            // string InputName = string.Format("{0}_{1}", CheatCommand.command.baseName, InArgAttr.index);
            string InputName = string.Format("Argument_{0}", GUIUtility.GetControlID(FocusType.Keyboard));

            GUI.SetNextControlName(InputName);

            OutValue = GUILayout.TextField(OutValue, ParentView.CustomTextFieldStyle, GUILayout.Width(200));

            string FocusedName = GUI.GetNameOfFocusedControl();

            if (FocusedName == InputName)
            {
                // let try get some helper for this argument
                IArgumentDescription ArgDescInterface = ArgumentDescriptionRepository.instance.GetDescription(InArgAttr.argumentType);

                DebugHelper.Assert(ArgDescInterface != null);

                List<String> Results = ArgDescInterface.FilteredCandinates(InArgAttr.argumentType, OutValue);

                if (Results != null && Results.Count > 0)
                {
                    for (int i = 0; i < Results.Count; ++i)
                    {
                        string CandinateName = Results[i];

                        if (!CandinateName.Equals(OutValue, StringComparison.InvariantCultureIgnoreCase)
                            && DrawButton(CandinateName)
                            )
                        {
                            OutValue = CandinateName;
                            break;
                        }
                    }
                }
            }

            GUILayout.EndHorizontal();

            return true;
        }

        private bool ShouldSkip(ArgumentDescriptionAttribute InArgAttr, ref string[] ExistsValues)
        {
            DebugHelper.Assert(InArgAttr.isOptional);

            DependencyDescription[] Dependencies = InArgAttr.depends;

            for (int i = 0; i < Dependencies.Length; ++i)
            {
                DependencyDescription Depend = Dependencies[i];

                string ExistsValue = ExistsValues[Depend.dependsIndex];

                var DependType = CheatCommand.argumentsTypes[Depend.dependsIndex].argumentType;

                var ArgDescInterface = ArgumentDescriptionRepository.instance.GetDescription(DependType);

                DebugHelper.Assert(ArgDescInterface != null);

                ExistsValue = ArgDescInterface.GetValue(DependType, ExistsValue);

                if (Depend.ShouldBackOff(ExistsValue))
                {
                    return false;
                }
            }

            return true;
        }

        public override void OnStateEnter()
        {
            logger.Clear();
        }

        public override bool canScroll
        {
            get
            {
                return false;
            }
        }
    }

    class ConsoleViewMobile : IConsoleView
    {
        ConsoleWindow ParentWindow;

        MobileLogger Logger = new MobileLogger();

        StateMachine States = new StateMachine();

        ConsoleDragHandler DragHandler = new ConsoleDragHandler();
        
        public ConsoleViewMobile(ConsoleWindow InParent)
        {
            ParentWindow = InParent;

            States.RegisterStateByAttributes<CommandDisplayAttribute>(typeof(CommandDisplayAttribute).Assembly, ParentWindow, this);
            States.ChangeState("CommandGroupDisplayState");
        }

        public void Awake()
        {
        }

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }


        public Rect SelectWindowRect()
        {
            return new Rect(0, 0, Screen.width, Screen.height);
        }

        public IConsoleLogger logger { get { return Logger; } }

        public void OnEnter()
        {

        }

        public GUIStyle CustomButtonStyle = null;
        public GUIStyle CustomLabelStyle = null;
        public GUIStyle CustomSmallLabelStyle = null;
        public GUIStyle CustomTextFieldStyle = null;

        public int skipCount
        {
            get;
            private set;
        }

        private int MaxSkipCount;
        private float ScrollValue;

        public void UpdateSkipCount(int InMaxCount)
        {
            MaxSkipCount = InMaxCount;
            skipCount = Math.Min(InMaxCount-1, skipCount);
        }

        public void OnConsole(int InWindowID)
        {
            try
            {
                if (CustomButtonStyle == null)
                {
                    CustomButtonStyle = new GUIStyle(GUI.skin.button);
                    CustomLabelStyle = new GUIStyle(GUI.skin.label);
                    CustomSmallLabelStyle = new GUIStyle(GUI.skin.label);
                    CustomTextFieldStyle = new GUIStyle(GUI.skin.textField);
                }

                int BaseUniformFontSize = 60;
                int BaseUniformFontSmallSize = 25;
                float OffsetFactor = 0.75f;
                float height = (float)Screen.height;
                float width = (float)Screen.width;

                int UniformFontSize = (int)(BaseUniformFontSize * Math.Min(height / 800.0f, width / 1280.0f) * OffsetFactor);
                int UniformFontSmallSize = (int)(BaseUniformFontSmallSize * Math.Min(height / 800.0f, width / 1280.0f) * OffsetFactor);

                CustomButtonStyle.fontSize = UniformFontSize;
                CustomLabelStyle.fontSize = UniformFontSize;
                CustomSmallLabelStyle.fontSize = UniformFontSmallSize;
                CustomTextFieldStyle.fontSize = UniformFontSize;

                GUILayout.BeginScrollView(
                    Vector2.zero,
                    false, false,
                    GUILayout.Width(Screen.width),
                    GUILayout.Height(Screen.height)
                    );

                {
                    GUILayout.BeginVertical();

                    // GUILayout.Label(Logger.message, CustomLabelStyle);

                    {
                        GUILayout.BeginHorizontal();

                        if (hasPreviousState)
                        {
                            DrawPreviousButton();
                        }
                        
                        GUILayout.EndHorizontal();
                    }


                    if (States.TopState() != null)
                    {
                        (States.TopState() as CommandDisplayBasicState).OnGUI();
                    }

                    GUILayout.EndVertical();
                }

                GUILayout.EndScrollView();
            }
            catch (Exception e)
            {
                DebugHelper.Assert(false, string.Format("ConsoleViewMobile Exception: {0} , Stack: {1}", e.Message, e.StackTrace));
            }
        }

        public static void DrawEmptyLine(int lineCount)
        {
            for (int i = 0; i < lineCount; ++i)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(" ", (GUILayoutOption[])null);
                GUILayout.EndHorizontal();
            }
        }
        
        protected void DrawPreviousButton()
        {
            GUI.contentColor = Color.cyan;

            try
            {
                if ((States.TopState() as CommandDisplayBasicState).DrawButton("Previous"))
                {
                    skipCount = 0;
                    States.PopState();
                }
            }
            finally
            {
                GUI.contentColor = Color.white;
            }
        }

        protected bool hasPreviousState
        {
            get
            {
                return States.Count > 1;
            }
        }

        public void SelectGroup(CheatCommandGroup InGroup)
        {
            DebugHelper.Assert(InGroup != null);

            States.Push(new CommandInGroupDisplayState(ParentWindow, this));

            (States.TopState() as CommandInGroupDisplayState).SetGroup(InGroup);
        }

        public void SelectionCommand(ICheatCommand InCommand)
        {
            DebugHelper.Assert(InCommand != null);

            States.Push(new CommandDisplayState(ParentWindow, this));

            (States.TopState() as CommandDisplayState).ResetCheatCommand(InCommand);
        }

        public void OnToggleVisible(bool bVisible)
        {
        }

        public void OnDestory()
        {

        }

        public void OnUpdate()
        {
            if (((CommandDisplayBasicState)States.TopState()).canScroll)
            {
                DragHandler.OnUpdate();
                if (DragHandler.isDragging)
                {
                    // Debug.Log(DragHandler.dragDelta);
#if UNITY_EDITOR || UNITY_STANDALONE
                    float Speed = 0.6f;
#else
                    float Speed = 10.0f;
#endif

                    float DragDelta = DragHandler.dragDelta * Speed;
                    ScrollValue += DragDelta;

                  //  Debug.Log(string.Format("drag delta:{0} scroll {1}", DragDelta, ScrollValue));

                    if (Math.Abs(ScrollValue) > 1)
                    {
                        int NewSkipCount = skipCount + (int)ScrollValue;
                        ScrollValue -= (int)ScrollValue;

                        NewSkipCount = Math.Min(MaxSkipCount-1, NewSkipCount);
                        skipCount = Math.Max(NewSkipCount, 0);
                    }
                }
                else
                {
                    ScrollValue = 0;
                }
            }
            else
            {
                ScrollValue = 0;
            }
        }
    }
}
#endif
/**
 * @brief PC Mode Drawer
 * @email dbdongbo@vip.qq.com
*/

#if !WITH_OUT_CHEAT_CONSOLE && (UNITY_STANDALONE || UNITY_EDITOR)
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Common;

namespace Assets.Scripts.Console
{
    class ConsoleLogger : IConsoleLogger
    {
        private string LogText = "";

        public string message
        {
            get
            {
                return LogText;
            }
        }

        public void AddMessage(string InMessage)
        {
            LogText = string.Format("{0}\n{1}", LogText, InMessage);
        }

        public void Clear()
        {
            LogText = "";
        }
    }

    class CandinateStateAttribute : AutoRegisterAttribute
    {
    }

    abstract class CandinatesState : BaseState
    {
        protected ConsoleViewPC ParentView = null;
        protected ItemList CandinatesList = new ItemList();

        public CandinatesState(ConsoleViewPC InParent)
        {
            ParentView = InParent;

            CandinatesList.ClickEvent += OnClick;
        }

        public ItemList candinateList
        {
            get
            {
                DebugHelper.Assert(CandinatesList != null);

                return CandinatesList;
            }
        }

        public string inputText
        {
            get { return ParentView.inputText; }
            set { ParentView.inputText = value; }
        }

        public string baseCommand
        {
            get
            {
                return ParentView.parser.baseCommand;
            }
        }

        public string[] arguments
        {
            get
            {
                return ParentView.parser.arguments;
            }
        }

        public void TryMoveSelection(int InDelta)
        {
            if (CandinatesList != null)
            {
                CandinatesList.TryMoveSelection(InDelta);
            }
        }

        public virtual void PreGUI() { }

        public abstract void OnGUI();

        public virtual bool HandleAutoComplete()
        {
            return false;
        }

        public virtual bool HandleSubmit(string InText, string InCommand, string[] InArguments)
        {
            return false;
        }

        protected virtual void OnClick(int Index)
        {
        }
    }

    class CheatCommandItem : IListItem
    {
        public ICheatCommand command { get; protected set; }

        public CheatCommandItem(ICheatCommand InCommand)
        {
            command = InCommand;
        }

        public string name { get { return command != null ? command.fullyHelper : ""; } }
    }

    [CandinateStateAttribute]
    class CommandCandinateState : CandinatesState
    {
        public CommandCandinateState(ConsoleViewPC InParent) :
            base(InParent)
        {
        }

        public override void OnGUI()
        {
            if (CandinatesList != null)
            {
                UpdateCandinates();

                var LastRect = GUILayoutUtility.GetLastRect();
                CandinatesList.OnGUI(LastRect);
            }

            CheckCandinateState();
        }

        public void CheckCandinateState()
        {
            ICheatCommand Command = CheatCommandsRepository.instance.FindCommand(baseCommand);

            if (Command == null)
            {
                return;
            }

            string InputText = inputText.TrimStart(' ');

            if (InputText.IndexOf(' ') != -1)
            {
                ParentView.ChangeState("ArgumentCandinateState");
            }
        }

        public void UpdateCandinates()
        {
            var InputText = inputText;
            string[] SplitedParts = LinqS.Where(InputText.Split(' '), x => !string.IsNullOrEmpty(x.Trim()));

            List<IListItem> Candinates = new List<IListItem>();

            if (SplitedParts == null || SplitedParts.Length <= 0)
            {
                // no any valid input.
                var GeneralRepositories = CheatCommandsRepository.instance.generalRepositories;

                var Iter = GeneralRepositories.Commands.GetEnumerator();

                while (Iter.MoveNext())
                {
                    Candinates.Add(new CheatCommandItem(Iter.Current.Value));
                }

                CandinatesList.Reset(Candinates);
            }
            else
            {
                var BaseCommand = SplitedParts[0];

                var FilteredCommands = CheatCommandsRepository.instance.FilterByString(BaseCommand);

                for (int i = 0; i < FilteredCommands.Count; ++i)
                {
                    Candinates.Add(new CheatCommandItem(FilteredCommands[i]));
                }

                CandinatesList.Reset(Candinates);
            }
        }


        public override bool HandleSubmit(string InText, string InCommand, string[] InArguments)
        {
            // search InCommand in list
            if (CandinatesList.hasValidSelection)
            {
                if (!((CheatCommandItem)CandinatesList.SelectedItem).command.command.baseName.Equals(InCommand, StringComparison.CurrentCultureIgnoreCase))
                {
                    ParentView.shouldUpdateCursor = HandleAutoComplete();

                    return false;
                }
            }

            return true;
        }

        public override bool HandleAutoComplete()
        {
            if (CandinatesList.hasValidSelection)
            {
                var Selection = CandinatesList.SelectedName;

                string[] SplitedParts = LinqS.Where(Selection.Split(' '), x => !string.IsNullOrEmpty(x.Trim()));

                if (SplitedParts != null && SplitedParts.Length > 0)
                {
                    inputText = SplitedParts[0];
                    return true;
                }
            }

            return false;
        }
    }

    class StringItem : IListItem
    {
        string Name;

        public StringItem(string InName)
        {
            Name = InName;
        }

        public string name { get { return Name; } }
    }

    [CandinateStateAttribute]
    class ArgumentCandinateState : CandinatesState
    {
        ICheatCommand CurrentCommand = null;

        string ErrorMessage = "";

        public ArgumentCandinateState(ConsoleViewPC InParent) :
            base(InParent)
        {
        }

        public override void OnStateEnter()
        {
            ErrorMessage = "";
        }

        public override void PreGUI()
        {
            CurrentCommand = CheatCommandsRepository.instance.FindCommand(baseCommand);

            if (CurrentCommand != null)
            {
                DrawHelperText(CurrentCommand);
                DrawDynamicCheckResult(CurrentCommand);
            }
        }

        public override void OnGUI()
        {
            if (!CheckCandinateState(CurrentCommand))
            {
                return;
            }

            DebugHelper.Assert(CurrentCommand != null);

            UpdateCandinates();

            CandinatesList.OnGUI(GUILayoutUtility.GetLastRect());
        }

        protected void UpdateCandinates()
        {
            bool bShouldResetNull = true;

            if (arguments != null)
            {
                // try auto complete arguments
                int Index = arguments.Length - 1;

                var ArgumentTypes = CurrentCommand.argumentsTypes;

                if (ArgumentTypes != null && Index < ArgumentTypes.Length)
                {
                    var ArgumentType = ArgumentTypes[Index];

                    IArgumentDescription ArgDescInterface = ArgumentDescriptionRepository.instance.GetDescription(ArgumentType.argumentType);

                    DebugHelper.Assert(ArgDescInterface != null);

                    List<String> Results = ArgDescInterface.FilteredCandinates(ArgumentType.argumentType, arguments[Index]);

                    ResetCandinates(Results);

                    bShouldResetNull = false;
                }
            }

            if (bShouldResetNull)
            {
                CandinatesList.Reset(null);
            }
        }

        protected void ResetCandinates(List<string> InValues)
        {
            if (InValues != null && InValues.Count > 0)
            {
                DebugHelper.Assert(arguments.Length > 0);

                // string ArgInput = arguments[arguments.Length - 1];

                List<IListItem> Items = new List<IListItem>(InValues.Count);

                for (int i = 0; i < InValues.Count; ++i)
                {
                    Items.Add(new StringItem(InValues[i]));
                }

                CandinatesList.Reset(Items);
            }
            else
            {
                CandinatesList.Reset(null);
            }
        }

        protected void DrawLabel(string InText)
        {
            var Content = new GUIContent(InText);

            var Size = GUI.skin.label.CalcSize(Content);

            GUILayout.Label(Content, GUILayout.Width(Size.x));
        }

        protected void DrawHelperText(ICheatCommand InCommand)
        {
            // GUILayout.Label(InCommand.fullyHelper);
            GUILayout.BeginHorizontal();
            DrawLabel(InCommand.command.baseName + " ");
            int CurrentArgIndex = arguments != null && arguments.Length > 0 ? arguments.Length - 1 : -1;

            if (InCommand.argumentsTypes != null)
            {
                for (int i = 0; i < InCommand.argumentsTypes.Length; ++i)
                {
                    Type ThisType = InCommand.argumentsTypes[i].argumentType;

                    string ArgDesc = string.Format(" <{0}{2}|{1}>", InCommand.argumentsTypes[i].name, ThisType.Name, InCommand.argumentsTypes[i].isOptional ? "(Optional)" : "");

                    if (CurrentArgIndex == i)
                    {
                        GUI.contentColor = Color.green;
                    }

                    DrawLabel(ArgDesc);

                    if (CurrentArgIndex == i)
                    {
                        GUI.contentColor = Color.white;
                    }
                }
            }

            DrawLabel(string.Format(" Desc: {0}", InCommand.comment));

            GUILayout.EndHorizontal();
        }

        protected void DrawDynamicCheckResult(ICheatCommand InCommand)
        {
            GUI.contentColor = Color.yellow;

            // update error codes.
            InCommand.CheckArguments(ParentView.parser.arguments, out ErrorMessage);

            GUILayout.Label(ErrorMessage);

            GUI.contentColor = Color.white;
        }

        protected bool CheckCandinateState(ICheatCommand InCommand)
        {
            string InputText = inputText.TrimStart(' ');

            if (InCommand == null || InputText.IndexOf(' ') == -1)
            {
                ParentView.ChangeState("CommandCandinateState");
                return false;
            }

            return true;
        }

        public override bool HandleSubmit(string InText, string InCommand, string[] InArguments)
        {
            // search InCommand in list
            if (CandinatesList.hasValidSelection)
            {
                var Selection = candinateList.SelectedName;

                if (arguments != null &&
                    arguments.Length > 0 &&
                    arguments[arguments.Length - 1] != Selection
                    )
                {
                    ParentView.shouldUpdateCursor = HandleAutoComplete();

                    return false;
                }
            }

            return true;
        }

        public override bool HandleAutoComplete()
        {
            if (CandinatesList.hasValidSelection)
            {
                var Selection = CandinatesList.SelectedName;

                int Index = ParentView.inputText.LastIndexOf(' ');

                DebugHelper.Assert(Index != -1);

                if (Index < ParentView.inputText.Length - 1)
                {
                    ParentView.inputText = ParentView.inputText.Remove(Index + 1);
                }

                ParentView.inputText += Selection;

                return true;
            }

            return false;
        }
    }


    class ConsoleViewPC : IConsoleView
    {
        ConsoleWindow ParentWindow;

        ConsoleLogger Logger = new ConsoleLogger();

        CommandHistory CommandLogger = new CommandHistory();

        StateMachine CandinateContext = new StateMachine();

        string InputText = "";

        bool bFocused = false;

        bool bShouldUpdateCursorPos = false;

        public int cursorPos { get; protected set; }

        public CommandParser parser { get; protected set; }

        public ConsoleViewPC(ConsoleWindow InParent)
        {
            ParentWindow = InParent;
            parser = new CommandParser();

            CandinateContext.RegisterStateByAttributes<CandinateStateAttribute>(typeof(CandinateStateAttribute).Assembly, this);
            CandinateContext.ChangeState("CommandCandinateState");

            CommandLogger.Load();
        }

        public void Awake()
        {
        }

        public void OnEnable()
        {
            bFocused = true;
        }

        public void OnDisable()
        {
            bFocused = true;
        }

        public Rect SelectWindowRect()
        {
            return new Rect(0, 0, Screen.width, Screen.height);
        }

        public IConsoleLogger logger { get { return Logger; } }
        public bool shouldUpdateCursor
        {
            get { return bShouldUpdateCursorPos; }
            set { bShouldUpdateCursorPos = value; }
        }

        public string inputText
        {
            get
            {
                return InputText;
            }
            set
            {
                InputText = value;
            }
        }

        public void OnEnter()
        {
            bFocused = true;
        }

        private void HandleSubmit()
        {
            if (KeyDown("[enter]") || KeyDown("return"))
            {
                if (!string.IsNullOrEmpty(parser.baseCommand))
                {
                    if ((CandinateContext.TopState() as CandinatesState).HandleSubmit(InputText, parser.baseCommand, parser.arguments))
                    {
                        Logger.AddMessage("> " + InputText);

                        if (CheatCommandsRepository.instance.HasCommand(parser.baseCommand))
                        {
                            Logger.AddMessage(CheatCommandsRepository.instance.ExecuteCommand(parser.baseCommand, parser.arguments));

                            CommandLogger.PushCommand(parser.baseCommand, inputText);
                        }
                        else
                        {
                            Logger.AddMessage(string.Format("Command {0} not found.", parser.baseCommand));
                        }

                        InputText = "";
                    }
                }
                else
                {
                    bShouldUpdateCursorPos = (CandinateContext.TopState() as CandinatesState).HandleAutoComplete();
                }
            }
        }

        private void HandleEscape()
        {
            if (KeyDown("escape") || KeyDown("F1"))
            {
                ParentWindow.isVisible = false;

                InputText = "";
            }
        }

        private void HandleAutoComplete()
        {
            if (KeyDown("tab"))
            {
                bShouldUpdateCursorPos = (CandinateContext.TopState() as CandinatesState).HandleAutoComplete();
            }
        }

        private void HandleLogSelection(CommandLog InResult, bool bUseFullData)
        {
            if (InResult != null)
            {
                inputText = bUseFullData ? InResult.FullCommand : InResult.BaseCommand;
                bShouldUpdateCursorPos = true;
            }
        }

        private void HandleArrowSelection()
        {
            if (KeyDown(KeyCode.UpArrow))
            {
                if (Event.current.control || Event.current.command)
                {
                    HandleLogSelection(CommandLogger.previousCommand, !Event.current.shift);
                }
                else
                {
                    (CandinateContext.TopState() as CandinatesState).TryMoveSelection(-1);
                }
            }
            else if (KeyDown(KeyCode.DownArrow))
            {
                if (Event.current.control || Event.current.command)
                {
                    HandleLogSelection(CommandLogger.nextCommand, !Event.current.shift);
                }
                else
                {
                    (CandinateContext.TopState() as CandinatesState).TryMoveSelection(1);
                }
            }
        }

        private bool KeyDown(string key)
        {
            return Event.current.Equals(Event.KeyboardEvent(key));
        }

        private bool KeyDown(KeyCode InCode)
        {
            return Event.current.keyCode == InCode && Event.current.type == EventType.keyDown;
        }

        public void OnConsole(int InWindowID)
        {
            HandleSubmit();
            HandleEscape();
            HandleAutoComplete();
            HandleArrowSelection();

            GUILayout.BeginScrollView(Vector2.zero,
                false,
                false,
                GUILayout.Width(Screen.width),
                GUILayout.Height(Screen.height * 0.618f)
                );

            GUILayout.Label(Logger.message);
            GUILayout.EndScrollView();

            (CandinateContext.TopState() as CandinatesState).PreGUI();

            GUI.SetNextControlName("InputField");

            InputText = GUILayout.TextField(InputText);

            parser.Parse(InputText);

            TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

#if UNITY_5
            cursorPos = editor.cursorIndex;
#else
            cursorPos = editor.pos;
#endif

            if (bShouldUpdateCursorPos)
            {
                bShouldUpdateCursorPos = false;

                // editor.selectPos = InputText.Length;
                editor.MoveGraphicalLineEnd();
            }

            if (bFocused)
            {
                GUI.FocusControl("InputField");

                bFocused = false;
            }

            (CandinateContext.TopState() as CandinatesState).OnGUI();
        }

        public void ChangeState(string stateName)
        {
            CandinateContext.ChangeState(stateName);
        }

        public void OnToggleVisible(bool bVisible)
        {
            bFocused = bVisible;

            if (!bVisible)
            {
                CommandLogger.Save();
            }
        }

        public void OnDestory()
        {
            CommandLogger.Save();
        }

        public void OnUpdate()
        {

        }
    }
}
#endif
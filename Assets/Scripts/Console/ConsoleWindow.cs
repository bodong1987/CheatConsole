/**
 * @brief Console Window Root
 * @email dbdongbo@vip.qq.com
*/

#if !WITH_OUT_CHEAT_CONSOLE

using Assets.Scripts.Common;
using UnityEngine;

namespace Assets.Scripts.Console
{
    public interface IConsoleLogger
    {
        // get all log text
        string message { get; }

        // add new log message
        void AddMessage(string InMessage);

        // clear all log text
        void Clear();
    }

    public interface IConsoleView
    {
        IConsoleLogger logger { get; }

        void Awake();
        void OnEnable();
        void OnDisable();

        void OnEnter();

        Rect SelectWindowRect();

        void OnConsole(int InWindowID);

        void OnToggleVisible(bool bVisible);

        void OnDestory();

        void OnUpdate();
    }

    public class ConsoleWindow : MonoSingleton<ConsoleWindow>
    {
        static int InternalID = 0x00dbdbdb;

        protected IConsoleView Viewer = null;

        Rect WindowRect;

        bool bShouldVisible = false;

        public bool bEnableCheatConsole = false;

        public IConsoleLogger externalLogger = null;

#if UNITY_4_6 || UNITY_5
        // this can disable penetrate this window
        protected UnityEngine.EventSystems.EventSystem CachedEventSystem;
#endif

        protected override void Init()
        {
            CheatCommandRegister.GetInstance();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (Viewer != null)
            {
                Viewer.OnDestory();
            }
        }

        void Update()
        {
            if (!bEnableCheatConsole)
            {
                return;
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetKeyDown(KeyCode.F1))
            {
                bool bVisible = ToggleVisible();

                if (bVisible)
                {
                    if (Input.GetKey(KeyCode.LeftControl) ||
                        Input.GetKey(KeyCode.LeftCommand))
                    {
                        ChangeToMobileView();
                    }
                    else
                    {
                        ChangeToPCView();
                    }
                }
            }
#endif

            for (int i = 0; i < Input.touchCount; ++i)
            {
                var CurTouch = Input.GetTouch(i);

                if (CurTouch.fingerId == 4 && CurTouch.phase == TouchPhase.Began)
                {
                    ToggleVisible();
                    break;
                }
            }

            if (Viewer != null)
            {
                Viewer.OnUpdate();
            }
        }


        public void ChangeToMobileView()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            if (Viewer as ConsoleViewMobile == null)
            {
                Viewer = new ConsoleViewMobile(this);
                Viewer.OnEnter();
            }
#endif
        }

        public void ChangeToPCView()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            if (Viewer as ConsoleViewPC == null)
            {
                Viewer = new ConsoleViewPC(this);
                Viewer.OnEnter();
            }
#endif
        }

        public bool ToggleVisible()
        {
            bool bResult = !isVisible && bEnableCheatConsole;

            isVisible = bResult;

            return bResult;
        }

        protected override void Awake()
        {
            base.Awake();

#if UNITY_STANDALONE || UNITY_EDITOR
            Viewer = new ConsoleViewPC(this);
#elif UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8
            Viewer = new ConsoleViewMobile(this);
#endif

            if (Viewer != null)
            {
                Viewer.Awake();
            }
        }

        void OnEnable()
        {
            if (Viewer != null)
            {
                Viewer.OnEnable();
            }
        }

        void OnDisable()
        {
            if (Viewer != null)
            {
                Viewer.OnDisable();
            }
        }

        void OnGUI()
        {
            if (Viewer == null || !isVisible)
            {
                return;
            }

            WindowRect = Viewer.SelectWindowRect();

            GUILayout.Window(InternalID, WindowRect, OnConsole, "CheatConsole");
        }

        private void OnConsole(int InWindowID)
        {
            DebugHelper.Assert(Viewer != null);

            if (Viewer != null)
            {
                Viewer.OnConsole(InWindowID);
            }
        }

        public void ClearLog()
        {
            if (Viewer != null && Viewer.logger != null)
            {
                Viewer.logger.Clear();
            }

            if (externalLogger != null)
            {
                externalLogger.Clear();
            }
        }

        public void AddMessage(string InMessage)
        {
            if (Viewer != null && Viewer.logger != null)
            {
                Viewer.logger.AddMessage(InMessage);
            }

            if (externalLogger != null)
            {
                externalLogger.AddMessage(InMessage);
            }
        }

        /**
         * @todo 当isVisible被设置为true时，弹出一个UIForm把所有的UI输入吞掉 
        */
        public bool isVisible
        {
            get
            {
                return bShouldVisible;
            }
            set
            {
                bShouldVisible = value;

                if (Viewer != null)
                {
                    Viewer.OnToggleVisible(bShouldVisible);
                }

#if UNITY_4_6 || UNITY_5
                if (CachedEventSystem == null)
                {
                    CachedEventSystem = UnityEngine.EventSystems.EventSystem.current;
                }

                if (CachedEventSystem != null)
                {
                    CachedEventSystem.enabled = !value;
                }
#endif
            }
        }
    }
}
#endif
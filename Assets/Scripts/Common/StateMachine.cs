using System.Collections.Generic;
using System.Reflection;


namespace Assets.Scripts.Common
{
    /// <summary>
    ///     状态接口
    /// </summary>
    public interface IState
    {
        /// <summary>
        ///     状态进入状态栈
        /// </summary>
        void OnStateEnter();

        /// <summary>
        ///     状态退出状态栈
        /// </summary>
        void OnStateLeave();

        /// <summary>
        ///     状态由栈顶变成非栈顶
        /// </summary>
        void OnStateOverride();

        /// <summary>
        ///     状态由非栈顶变成栈顶
        /// </summary>
        void OnStateResume();

        string name { get; }
    }

    public abstract class BaseState : IState
    {
        public virtual void OnStateEnter() { }

        public virtual void OnStateLeave() { }

        public virtual void OnStateOverride() { }

        public virtual void OnStateResume() { }

        public virtual string name { get { return GetType().Name; } }
    }

    /// <summary>
    ///     状态机
    /// </summary>
    public class StateMachine
    {
        // 状态名-已注册的状态
        private DictionaryView<string, IState> _registedState = new DictionaryView<string, IState>();

        // 状态堆栈
        private Stack<IState> _stateStack = new Stack<IState>();

        public IState tarState { get; private set; }

        public void RegisterState(string name, IState state)
        {
            if (name == null || state == null)
            {
                return;
            }

            if (_registedState.ContainsKey(name))
            {
                return;
            }

            _registedState.Add(name, state);
        }

        public ClassEnumerator RegisterStateByAttributes<TAttributeType>(Assembly InAssembly, params object[] args)
            where TAttributeType : AutoRegisterAttribute
        {
            var Classes = new ClassEnumerator(
                typeof(TAttributeType),
                typeof(IState),
                InAssembly);

            var Iter = Classes.results.GetEnumerator();

            while (Iter.MoveNext())
            {
                var StateType = Iter.Current;

                IState StateObj = (IState)System.Activator.CreateInstance(StateType, args);

                RegisterState(StateObj, StateObj.name);
            }

            return Classes;
        }

        public ClassEnumerator RegisterStateByAttributes<TAttributeType>(Assembly InAssembly)
            where TAttributeType : AutoRegisterAttribute
        {
            var Classes = new ClassEnumerator(
                typeof(TAttributeType),
                typeof(IState),
                InAssembly);

            var Iter = Classes.results.GetEnumerator();

            while (Iter.MoveNext())
            {
                var StateType = Iter.Current;

                IState StateObj = (IState)System.Activator.CreateInstance(StateType);

                RegisterState(StateObj, StateObj.name);
            }

            return Classes;
        }

        public void RegisterState<TStateImplType>(TStateImplType State, string name)
            where TStateImplType : IState
        {
            RegisterState(name, State);
        }

        /// <summary>
        ///     注销状态
        /// </summary>
        /// <param name="name">状态名</param>
        /// <returns>被注销的状态</returns>
        public IState UnregisterState(string name)
        {
            if (name == null)
            {
                return default(IState);
            }

            IState state;
            if (!_registedState.TryGetValue(name, out state))
            {
                return default(IState);
            }

            _registedState.Remove(name);

            return state;
        }

        /// <summary>
        /// 获取状态
        /// </summary>
        /// <param name="name">状态名</param>
        /// <returns>状态</returns>
        public IState GetState(string name)
        {
            if (null == name)
            {
                return default(IState);
            }

            IState state;
            return _registedState.TryGetValue(name, out state) ? state : default(IState);
        }

        public string GetStateName(IState state)
        {
            if (null == state)
            {
                return null;
            }

            var etr = _registedState.GetEnumerator();
            KeyValuePair<string, IState> pair;
            while (etr.MoveNext())
            {
                pair = etr.Current;

                if (pair.Value == state)
                {
                    return pair.Key;
                }
            }

            return null;
        }

        /// <summary>
        ///     压入状态
        /// </summary>
        /// <param name="state">状态</param>
        public void Push(IState state)
        {
            if (state == null)
            {
                return;
            }

            if (_stateStack.Count > 0)
            {
                _stateStack.Peek().OnStateOverride();
            }

            _stateStack.Push(state);

            state.OnStateEnter();
        }

        /// <summary>
        ///     压入状态
        /// </summary>
        /// <param name="name">状态名</param>
        public void Push(string name)
        {
            if (name == null)
            {
                return;
            }

            IState state;
            if (!_registedState.TryGetValue(name, out state))
            {
                return;
            }

            Push(state);
        }

        /// <summary>
        ///     弹出状态
        /// </summary>
        /// <returns>被弹出的状态</returns>
        public IState PopState()
        {
            if (_stateStack.Count <= 0)
            {
                return default(IState);
            }

            IState state = _stateStack.Pop();
            state.OnStateLeave();

            if (_stateStack.Count > 0)
            {
                _stateStack.Peek().OnStateResume();
            }

            return state;
        }

        /// <summary>
        ///     修改栈顶状态
        /// </summary>
        /// <param name="state">新栈顶状态</param>
        /// <returns>原栈顶状态</returns>
        public IState ChangeState(IState state)
        {
            if (state == null)
            {
                return default(IState);
            }

            tarState = state;

            IState oldState = default(IState);
            if (_stateStack.Count > 0)
            {
                oldState = _stateStack.Pop();
                oldState.OnStateLeave();
            }

            _stateStack.Push(state);
            state.OnStateEnter();

            return oldState;
        }

        /// <summary>
        /// 修改栈顶状态
        /// </summary>
        /// <param name="name">新栈顶状态名</param>
        /// <returns>原栈顶状态</returns>
        public IState ChangeState(string name)
        {
            if (name == null)
            {                
                return default(IState);
            }

            IState state;
            if (!_registedState.TryGetValue(name, out state))
            {                
                return default(IState);
            }

            return ChangeState(state);
        }

        /// <summary>
        ///     获取栈顶状态
        /// </summary>
        /// <returns>栈顶状态</returns>
        public IState TopState()
        {
            if (_stateStack.Count <= 0)
            {
                return default(IState);
            }

            return _stateStack.Peek();
        }

        public string TopStateName()
        {
            if (_stateStack.Count <= 0)
            {
                return null;
            }

            IState state = _stateStack.Peek();
            return GetStateName(state);
        }

        /// <summary>
        ///     清空堆栈
        /// </summary>
        public void Clear()
        {
            while (_stateStack.Count > 0)
            {
                _stateStack.Pop().OnStateLeave();
            }
        }

        public int Count { get { return _stateStack.Count; } }
    }
}
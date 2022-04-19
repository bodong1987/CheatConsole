/**
 * @brief Register
 * @email dbdongbo@vip.qq.com
*/

#if !WITH_OUT_CHEAT_CONSOLE
using Assets.Scripts.Common;
using System;
using System.Reflection;
using System.Collections.Generic;


namespace Assets.Scripts.Console
{
    public class CheatCommandRegister : Singleton<CheatCommandRegister>
    {
        protected ListView<ICheatCommand> CommandRepositories = new ListView<ICheatCommand>();

        public void Register(Assembly InAssembly)
        {
            RegisterCommonCommands(InAssembly);
            RegisterMethodCommands(InAssembly);
        }

        protected void RegisterCommonCommands(Assembly InAssembly)
        {
            var TestAssembly = InAssembly;

            Type[] Types = TestAssembly.GetTypes();

            for (int ti = 0; Types != null && ti < Types.Length; ++ti)
            {
                var t = Types[ti];

                object[] Attributes = t.GetCustomAttributes(typeof(CheatCommandAttribute), true);

                if (Attributes != null)
                {
                    for (int i = 0; i < Attributes.Length; ++i)
                    {
                        // test in this type
                        CheatCommandAttribute Attr = Attributes[i] as CheatCommandAttribute;

                        if (Attr != null)
                        {
                            OnFoundClass(Attr.ID, t);
                        }
                    }
                }
            }
        }

        protected void OnFoundClass(string InID, Type InType)
        {
            CheatCommandAttribute Attribute = InType.GetCustomAttributes(typeof(CheatCommandAttribute), false)[0] as CheatCommandAttribute;

            DebugHelper.Assert(Attribute != null);

            ICheatCommand CommandHandler = Activator.CreateInstance(InType) as ICheatCommand;

            DebugHelper.Assert(CommandHandler != null);

            CommandRepositories.Add(CommandHandler);

            CheatCommandsRepository.instance.RegisterCommand(CommandHandler);
        }

        protected void RegisterMethodCommands(Assembly InAssembly)
        {
            ClassEnumerator Enumerator =
                new ClassEnumerator(
                    typeof(CheatCommandEntryAttribute),
                    null,
                    InAssembly
                    );

            var Iter = Enumerator.results.GetEnumerator();

            while (Iter.MoveNext())
            {
                Type ThisType = Iter.Current;

                RegisterMethods(ThisType);
            }
        }

        protected void RegisterMethods(Type InType)
        {
            CheatCommandEntryAttribute EntryAttr = ((CheatCommandEntryAttribute)InType.GetCustomAttributes(typeof(CheatCommandEntryAttribute), false)[0]);

            DebugHelper.Assert(EntryAttr != null);

            MethodInfo[] Methods = InType.GetMethods();

            if (Methods != null)
            {
                var Iter = Methods.GetEnumerator();

                while (Iter.MoveNext())
                {
                    MethodInfo Method = (MethodInfo)Iter.Current;

                    if (Method.IsStatic)
                    {
                        var Attributes = Method.GetCustomAttributes(typeof(CheatCommandEntryMethodAttribute), false);

                        if (Attributes != null && Attributes.Length > 0 && ValidateMethodArguments(Method))
                        {
                            CheatCommandEntryMethodAttribute MethodAttr = (CheatCommandEntryMethodAttribute)Attributes[0];

                            RegisterMethod(
                                InType,
                                EntryAttr,
                                Method,
                                MethodAttr
                                );
                        }
                    }
                }
            }
        }

        protected bool ValidateMethodArguments(MethodInfo InMethod)
        {
            Type ReturnType = InMethod.ReturnType;

            if (ReturnType != typeof(string))
            {
                DebugHelper.Assert(false, "Method Command must return a string.");
                return false;
            }

            ParameterInfo[] Parameters = InMethod.GetParameters();

            if (Parameters != null && Parameters.Length > 0)
            {
                for (int i = 0; i < Parameters.Length; ++i)
                {
                    ParameterInfo ParameterType = Parameters[i];

                    if (ParameterType.IsOut)
                    {
                        DebugHelper.Assert(false,
                            string.Format(
                            "method command argument can't be out parameter. Method:{0}, Parameter:{1} {2}",
                            InMethod.Name,
                            ParameterType.ParameterType.Name,
                            ParameterType.Name)
                            );

                        return false;
                    }

                    /*
                        if (ParameterType.IsOptional)
                        {
                            DebugHelper.Assert(false,
                                string.Format(
                                "method command argument can't be optional. Method:{0}, Parameter:{1} {2}",
                                InMethod.Name,
                                ParameterType.ParameterType.Name,
                                ParameterType.Name
                                )
                                );

                            return false;
                        }
                    */

                    if (ParameterType.ParameterType.IsByRef)
                    {
                        DebugHelper.Assert(false,
                            string.Format(
                            "method command argument can't be ref parameter. Method:{0}, Parameter:{1} {2}",
                            InMethod.Name,
                            ParameterType.ParameterType.Name,
                            ParameterType.Name)
                            );

                        return false;
                    }

                    IArgumentDescription ArgDescInterface = ArgumentDescriptionRepository.instance.GetDescription(ParameterType.ParameterType);

                    DebugHelper.Assert(ArgDescInterface != null);

                    if (!ArgDescInterface.AcceptAsMethodParameter(ParameterType.ParameterType))
                    {
                        DebugHelper.Assert(false,
                            string.Format(
                            "unsupported argument type for method command. Method:{0}, {1}, {2}",
                            InMethod.Name,
                            ParameterType.ParameterType.Name,
                            ParameterType.Name
                            )
                            );

                        return false;
                    }
                }
            }

            return true;
        }

        protected void RegisterMethod(
            Type InEntryType,
            CheatCommandEntryAttribute InEntryAttr,
            MethodInfo InMethod,
            CheatCommandEntryMethodAttribute InMethodAttr
            )
        {
            CheatCommandMethod MethodCommand =
                new CheatCommandMethod(InMethod, InEntryAttr, InMethodAttr);

            CheatCommandsRepository.instance.RegisterCommand(MethodCommand);
        }
    }
}

#endif
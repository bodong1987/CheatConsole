/**
 * @brief Basic Implementation
 * @email bodong@tencent.com
*/
#if !WITH_OUT_CHEAT_CONSOLE

using Assets.Scripts.Common;
using System;
using System.ComponentModel;
using System.Reflection;

namespace Assets.Scripts.Console
{
    public interface ICheatCommand
    {
        CheatCommandName command { get; }

        string comment { get; }

        ArgumentDescriptionAttribute[] argumentsTypes { get; }

        string description { get; }

        string fullyHelper { get; }

        string StartProcess(string[] InArguments);

        bool CheckArguments(string[] InArguments, out string OutMessage);

        bool isSupportInEditor { get; }

        bool isHiddenInMobile { get; }

        // used for editor only, cache arguments.
        string[] arguments { get; }
    }

    public abstract class CheatCommandBase : ICheatCommand
    {
        public static readonly string Done = @"done";

        private String[] Arguments = null;

        public CheatCommandBase()
        {
        }

        protected void ValidateArgumentsBuffer()
        {
            if (argumentsTypes != null && argumentsTypes.Length > 0)
            {
                Arguments = new string[argumentsTypes.Length];

                for (int i = 0; i < Arguments.Length; ++i)
                {
                    Arguments[i] = argumentsTypes[i].defaultValue;
                }
            }
        }

        private string GetArgumentNameAt(int Index)
        {
            DebugHelper.Assert(Index >= 0 && Index < argumentsTypes.Length);

            return argumentsTypes[Index].name;
        }

        protected virtual bool CheckDependencies(
            ArgumentDescriptionAttribute InArugmentDescription,
            DependencyDescription[] InDependencies,
            string[] InArguments,
            out string OutMessage
            )
        {
            OutMessage = "";

            if (InArguments == null)
            {
                OutMessage = "Missing parameters";
                return false;
            }

            for (int i = 0; i < InDependencies.Length; ++i)
            {
                DependencyDescription CurDependency = InDependencies[i];

                DebugHelper.Assert(CurDependency.dependsIndex >= 0 && CurDependency.dependsIndex < argumentsTypes.Length, "maybe internal error, can't find depend argument description.");

                if (CurDependency.dependsIndex < 0 || CurDependency.dependsIndex >= argumentsTypes.Length)
                {
                    OutMessage = "maybe internal error, can't find depend argument description.";

                    return false;
                }

                DebugHelper.Assert(CurDependency.dependsIndex < InArguments.Length);

                // try convert to actual value
                string CurDependValue = InArguments[CurDependency.dependsIndex];

                Type DependType = argumentsTypes[CurDependency.dependsIndex].argumentType;
                var ArgDescriptionInterface = ArgumentDescriptionRepository.instance.GetDescription(DependType);

                DebugHelper.Assert(ArgDescriptionInterface != null);

                CurDependValue = ArgDescriptionInterface.GetValue(DependType, CurDependValue);

                if (CurDependency.ShouldBackOff(CurDependValue))
                {
                    OutMessage = string.Format(
                    "you must provide parameter <{2}>, because <{0}>=\"{1}\"",
                        argumentsTypes[CurDependency.dependsIndex].name,
                        CurDependValue,
                        InArugmentDescription.name
                        );

                    return false;
                }
            }

            return true;
        }

        public virtual bool CheckArguments(string[] InArguments, out string OutMessage)
        {
            if (argumentsTypes != null && argumentsTypes.Length > 0)
            {
                for (int i = 0; i < argumentsTypes.Length; ++i)
                {
                    var ArgumentDescription = argumentsTypes[i];

                    if (InArguments == null || i >= InArguments.Length)
                    {
                        // missing parameters
                        if (!ArgumentDescription.isOptional)
                        {
                            // notify missing parameters                            
                            OutMessage = string.Format("Failed excecute command，missing parameter <{0}>, type:<{1}>", ArgumentDescription.name, ArgumentDescription.argumentType.Name);
                            return false;
                        }
                        else
                        {
                            // that is optional 
                            // so check dependency
                            DependencyDescription[] Dependencies = ArgumentDescription.depends;

                            if (Dependencies != null)
                            {
                                bool bResult = CheckDependencies(ArgumentDescription, Dependencies, InArguments, out OutMessage);

                                if (!bResult)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!ArgumentDescription.isOptional)
                        {
                            // just only check convert
                            string ErrorMessage;

                            if (!TypeCastCheck(InArguments[i], argumentsTypes[i], out ErrorMessage))
                            {
                                OutMessage = string.Format(
                                "Failed excecute command，parameter [{2}]=\"{0}\" can not convert to {1}, Error Message:{3}",
                                    InArguments[i],
                                    argumentsTypes[i].argumentType.Name,
                                    GetArgumentNameAt(i),
                                    ErrorMessage);

                                return false;
                            }
                        }
                        else
                        {
                            // that is optional 
                            // so check dependency
                            DependencyDescription[] Dependencies = ArgumentDescription.depends;

                            if (Dependencies != null)
                            {
                                bool bResult = CheckDependencies(ArgumentDescription, Dependencies, InArguments, out OutMessage);

                                if (!bResult)
                                {
                                    // must have this parameter
                                    // check it
                                    string ErrorMessage;

                                    if (!TypeCastCheck(InArguments[i], argumentsTypes[i], out ErrorMessage))
                                    {
                                        OutMessage = string.Format(
                                        "Failed excecute command, parameter [{2}]=\"{0}\" can't convert to {1}, Error Message:{3}",
                                            InArguments[i],
                                            argumentsTypes[i].argumentType.Name,
                                            GetArgumentNameAt(i),
                                            ErrorMessage);

                                        return false;
                                    }

                                    // if it is valid
                                    // skip
                                }

                                // not, always skip
                            }
                        }
                    }
                }
            }

            OutMessage = "";

            return true;
        }

        public virtual string StartProcess(string[] InArguments)
        {
            string ErrorMessage;

            if (!CheckArguments(InArguments, out ErrorMessage))
            {
                return ErrorMessage;
            }

            return Execute(InArguments);
        }

        protected abstract string Execute(string[] InArguments);

        public static T SmartConvert<T>(string InArgument)
        {
            var TypeConverter = TypeDescriptor.GetConverter(typeof(T));

            if (TypeConverter != null)
            {
                DebugHelper.Assert(TypeConverter.CanConvertFrom(typeof(string)));

                return (T)TypeConverter.ConvertFrom(InArgument);
            }

            return default(T);
        }

        public static bool TypeCastCheck(string InArgument, ArgumentDescriptionAttribute InArgDescription, out string OutErrorMessage)
        {
            DebugHelper.Assert(InArgDescription != null);

            return TypeCastCheck(InArgument, InArgDescription.argumentType, out OutErrorMessage);
        }

        public static bool TypeCastCheck(string InArgument, Type InType, out string OutErrorMessage)
        {
            IArgumentDescription DescriptionInterface = ArgumentDescriptionRepository.instance.GetDescription(InType);

            DebugHelper.Assert(DescriptionInterface != null);

            return DescriptionInterface.CheckConvert(InArgument, InType, out OutErrorMessage);
        }

        public static int StringToEnum(string InTest, Type InEnumType)
        {
            int Results = ArgumentDescriptionEnum.StringToEnum(InEnumType, InTest);

            return Results;
        }

        public virtual bool isSupportInEditor { get { return true; } }

        public virtual bool isHiddenInMobile { get { return false; } }

        public abstract CheatCommandName command { get; }

        public abstract string comment { get; }

        public abstract ArgumentDescriptionAttribute[] argumentsTypes { get; }

        public abstract int messageID { get; }

        public virtual string fullyHelper { get { return String.Format("{0} Desc: {1}", description, comment); } }

        public virtual string[] arguments { get { return Arguments; } }

        public virtual string description
        {
            get
            {
                string Arguments = "";

                if (argumentsTypes != null && argumentsTypes.Length > 0)
                {
                    for (int i = 0; i < argumentsTypes.Length; ++i)
                    {
                        Type ThisType = argumentsTypes[i].argumentType;

                        Arguments += string.Format(" <{0}{2}|{1}>", argumentsTypes[i].name, ThisType.Name, argumentsTypes[i].isOptional ? "(Optional)" : "");
                    }
                }

                return string.Format("{0}{1}", command.baseName, Arguments);
            }
        }
    }

    public abstract class CheatCommandCommon : CheatCommandBase
    {
        CheatCommandAttribute CachedAttribute;

        public CheatCommandCommon()
        {
            CacheAttribute();

            ValidateArgumentsBuffer();
        }

        protected void CacheAttribute()
        {
            var Attributes = GetType().GetCustomAttributes(typeof(CheatCommandAttribute), false);

            DebugHelper.Assert(Attributes != null && Attributes.Length > 0);

            CachedAttribute = Attributes[0] as CheatCommandAttribute;

            DebugHelper.Assert(CachedAttribute != null);

            if (CachedAttribute != null)
            {
                Attributes = GetType().GetCustomAttributes(typeof(ArgumentDescriptionAttribute), false);

                if (Attributes != null)
                {
                    CachedAttribute.IndependentInitialize(Attributes);
                }
            }
        }

        public override CheatCommandName command { get { return CachedAttribute.command; } }

        public override string comment { get { return CachedAttribute.comment; } }

        public override ArgumentDescriptionAttribute[] argumentsTypes { get { return CachedAttribute.argumentsTypes; } }

        public override int messageID { get { return CachedAttribute.messageID; } }
    }

    public class CheatCommandMethod : CheatCommandBase
    {
        MethodInfo Method;
        CheatCommandName CommandName;
        CheatCommandEntryMethodAttribute MethodAttr;
        string Comment;

        ArgumentDescriptionAttribute[] ArgumentDescs;

        class MethodCheatCommandName : CheatCommandName
        {
            public MethodCheatCommandName(string InName, string InBaseName) :
                base(InName)
            {
                baseName = InBaseName;
            }
        }

        public CheatCommandMethod(
            MethodInfo InMethod,
            CheatCommandEntryAttribute InEntryAttr,
            CheatCommandEntryMethodAttribute InMethodAttr)
        {
            Method = InMethod;
            CommandName = new MethodCheatCommandName(
                string.Format("{0}/{1}", InEntryAttr.group, InMethodAttr.comment),
                InMethod.Name
                );

            MethodAttr = InMethodAttr;

            String[] Comments = MethodAttr.comment.Split('/');
            Comment = Comments != null && Comments.Length > 0 ? Comments[Comments.Length - 1] : Method.Name;

            CacheArgumentDescriptions();

            ValidateArgumentsBuffer();
        }

        public override CheatCommandName command { get { return CommandName; } }

        public override string comment { get { return Comment; } }

        public override ArgumentDescriptionAttribute[] argumentsTypes { get { return ArgumentDescs; } }

        public override int messageID { get { return 0; } }

        public override bool isSupportInEditor
        {
            get
            {
                return MethodAttr.isSupportInEditor;
            }
        }

        public override bool isHiddenInMobile
        {
            get
            {
                return MethodAttr.isHiddenInMobile;
            }
        }

        protected void CacheArgumentDescriptions()
        {
            ParameterInfo[] Parameters = Method.GetParameters();

            if (Parameters != null && Parameters.Length > 0)
            {
                ArgumentDescs = new ArgumentDescriptionAttribute[Parameters.Length];

                for (int i = 0; i < ArgumentDescs.Length; ++i)
                {
                    ArgumentDescs[i] = new ArgumentDescriptionAttribute(
                    ArgumentDescriptionAttribute.EDefaultValueTag.Tag,
                        i,
                        Parameters[i].ParameterType,
                    Parameters[i].Name,
                    Parameters[i].DefaultValue != null ? Parameters[i].DefaultValue.ToString() : ""
                        );
                }
            }
        }

        protected override string Execute(string[] InArguments)
        {
            if (argumentsTypes == null || argumentsTypes.Length == 0)
            {
                return Method.Invoke(null, null) as string;
            }
            else
            {
                object[] Parameters = new object[argumentsTypes.Length];

                for (int i = 0; i < Parameters.Length; ++i)
                {
                    IArgumentDescription ArgDescInterface = ArgumentDescriptionRepository.instance.GetDescription(argumentsTypes[i].argumentType);

                    DebugHelper.Assert(ArgDescInterface != null);

                    Parameters[i] = ArgDescInterface.Convert(InArguments[i], argumentsTypes[i].argumentType);
                }

                return Method.Invoke(null, Parameters) as string;
            }
        }
    }
}
#endif
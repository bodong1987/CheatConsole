/**
 * @brief Command Description
 * @email bodong@tencent.com
*/

#if !WITH_OUT_CHEAT_CONSOLE
using Assets.Scripts.Common;
using System;
using System.Linq;

namespace Assets.Scripts.Console
{
    public class DependencyDescription
    {
        public int dependsIndex { get; protected set; }

        protected string[] Dpendencies = null;

        public DependencyDescription(int InIndex, string InValue)
        {
            dependsIndex = InIndex;

            Dpendencies = LinqS.Where(InValue.Split('|'), x => !string.IsNullOrEmpty(x.Trim()));
        }

        public bool ShouldBackOff(string InTest)
        {
            if (Dpendencies != null)
            {
                for (int i = 0; i < Dpendencies.Length; ++i)
                {
                    if (Dpendencies[i].Equals(InTest, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ArgumentDescriptionAttribute : Attribute
    {
        public int index { get; protected set; }

        public Type argumentType { get; protected set; }

        public Type commandLineType { get; protected set; }

        public string name { get; protected set; }

        public DependencyDescription[] depends { get; protected set; }

        public string defaultValue { get; protected set; }

        public enum EDefaultValueTag
        {
            Tag
        };

        public ArgumentDescriptionAttribute(
            Type InArgumentType,
            string InName,
                params object[] InDependencies
            ) :
            this(0, InArgumentType, InName, InDependencies)
        {
            defaultValue = "";
        }

        public ArgumentDescriptionAttribute(
            EDefaultValueTag InTag,
            int Index,
            Type InArgumentType,
            string InName,
            string InDefaultValue,
            params object[] InDependencies
            ) :
            this(Index, InArgumentType, InName, InDependencies)
        {
            defaultValue = InDefaultValue;
        }

        public ArgumentDescriptionAttribute(
            int Index,
            Type InArgumentType,
            string InName,
            params object[] InDependencies
            )
        {
            index = Index;
            argumentType = InArgumentType;
            name = InName;
            defaultValue = "";

            commandLineType = argumentType.IsEnum ? typeof(string) : argumentType;

            if (InDependencies != null && InDependencies.Length >= 2)
            {
                DebugHelper.Assert(InDependencies.Length % 2 == 0);

                int GroupCount = InDependencies.Length >> 1;
                depends = new DependencyDescription[GroupCount];

                for (int i = 0; i < GroupCount; ++i)
                {
                    depends[i] = new DependencyDescription(
                        Convert.ToInt32(InDependencies[i << 1]),
                        Convert.ToString(InDependencies[(i << 1) + 1])
                        );
                }
            }
        }

        public bool isOptional
        {
            get
            {
                return depends != null && depends.Length > 0;
            }
        }

        public bool isEnum
        {
            get
            {
                return argumentType.IsEnum;
            }
        }

        public void ValidateDependencies(int MaxIndex)
        {
            if (depends != null)
            {
                for (int i = 0; i < depends.Length; ++i)
                {
                    DebugHelper.Assert(depends[i].dependsIndex <= MaxIndex, "Invalid Dependencies!");
                }
            }
        }
    }

    public class CheatCommandName
    {
        public string baseName { get; protected set; }
        public string groupName { get; protected set; }
        public string rawName { get; protected set; }
        public string[] groupHierarchies { get; protected set; }

        public CheatCommandName(string InName)
        {
            DebugHelper.Assert(!string.IsNullOrEmpty(InName));

            rawName = InName;

            string[] SplitedResults = InName.Split('/');

            if (SplitedResults != null && SplitedResults.Length > 1)
            {
                baseName = SplitedResults[SplitedResults.Length - 1];
                groupName = SplitedResults[SplitedResults.Length - 2];
                groupHierarchies = LinqS.Take(SplitedResults, SplitedResults.Length - 1);
            }
            else
            {
                baseName = InName;
                groupName = @"Gernal";
                groupHierarchies = new string[] { groupName };
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CheatCommandAttribute :
        AutoRegisterAttribute,
        IIdentifierAttribute<string>
    {
        public CheatCommandName command { get; protected set; }

        public string comment { get; protected set; }

        public ArgumentDescriptionAttribute[] argumentsTypes { get; protected set; }

        public int messageID { get; protected set; }

        private bool bHasInitialized = false;

        public CheatCommandAttribute(
            string InExpression,
            string InComment,
            int InMessageID = 0
            )
        {
            command = new CheatCommandName(InExpression);
            comment = InComment;
            messageID = InMessageID;
        }

        private class ADAComparer : System.Collections.Generic.IComparer<ArgumentDescriptionAttribute>
        {
            public int Compare(ArgumentDescriptionAttribute x, ArgumentDescriptionAttribute y)
            {
                if (x.index < y.index)
                {
                    return -1;
                }
                else if (x.index == y.index)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
        }

        internal void IndependentInitialize(object[] InReferencesArguments)
        {
            if (!bHasInitialized)
            {
                bHasInitialized = true;
                argumentsTypes = new ArgumentDescriptionAttribute[InReferencesArguments.Length];

                for (int i = 0; i < argumentsTypes.Length; ++i)
                {
                    argumentsTypes[i] = InReferencesArguments[i] as ArgumentDescriptionAttribute;
                    DebugHelper.Assert(argumentsTypes[i] != null);
                }

                // order first

                Array.Sort(argumentsTypes, new ADAComparer());

                // check dependencies
                for (int i = 0; i < argumentsTypes.Length; ++i)
                {
                    argumentsTypes[i].ValidateDependencies(i - 1);
                }
            }
        }

        public string ID
        {
            get
            {
                return command.baseName;
            }
        }
    }

    public class CheatCommandEntryAttribute : AutoRegisterAttribute
    {
        public string group { get; protected set; }

        public CheatCommandEntryAttribute(string InGroup)
        {
            group = InGroup;
        }
    }

    public class CheatCommandEntryMethodAttribute : AutoRegisterAttribute
    {
        public string comment { get; protected set; }
        public bool isSupportInEditor { get; protected set; }
        public bool isHiddenInMobile { get; protected set; }

        public CheatCommandEntryMethodAttribute(
            string InComment,
            bool bInSupportInEditor,
            bool bHiddenInMobile = false
            )
        {
            comment = InComment;
            isSupportInEditor = bInSupportInEditor;
            isHiddenInMobile = bHiddenInMobile;
        }
    }
}
#endif
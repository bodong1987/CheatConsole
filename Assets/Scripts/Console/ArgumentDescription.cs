/**
 * @brief Argument Description
 * @email dbdongbo@vip.qq.com
*/

#if !WITH_OUT_CHEAT_CONSOLE
using Assets.Scripts.Common;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Assets.Scripts.Console
{
    public interface IArgumentDescription
    {
        bool Accept(Type InType);

        bool CheckConvert(string InArgument, Type InType, out string OutErrorMessage);

        string GetValue(Type InType, string InArgument);

        List<string> GetCandinates(Type InType);

        List<string> FilteredCandinates(Type InType, string InArgument);

        bool AcceptAsMethodParameter(Type InType);

        // assume you pass the CheckConvert check...
        object Convert(string InArgument, Type InType);
    }

    public class ArgumentAttribute : AutoRegisterAttribute
    {
        public int order { get; protected set; }

        public ArgumentAttribute(int InOrder)
        {
            order = InOrder;
        }
    }

    [ArgumentAttribute(-1)]
    public class ArgumentDescriptionDefault : IArgumentDescription
    {
        public bool Accept(Type InType)
        {
            return true;
        }

        public string GetValue(Type InType, string InArgument)
        {
            DebugHelper.Assert(InArgument!=null);

            return InArgument;
        }

        public bool CheckConvert(string InArgument, Type InType, out string OutErrorMessage)
        {
            return CheckConvertUtil(InArgument, InType, out OutErrorMessage);
        }

        public static bool CheckConvertUtil(string InArgument, Type InType, out string OutErrorMessage)
        {
            try
            {
                System.Convert.ChangeType(InArgument, InType);
                OutErrorMessage = "";

                return true;
            }
            catch (Exception e)
            {
                OutErrorMessage = e.Message;

                return false;
            }
        }

        public object Convert(string InArgument, Type InType)
        {
            try
            {
                return System.Convert.ChangeType(InArgument, InType);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<string> GetCandinates(Type InType)
        {
            return null;
        }

        public List<string> FilteredCandinates(Type InType, string InArgument)
        {
            return null;
        }

        public bool AcceptAsMethodParameter(Type InType)
        {
            return InType == typeof(string) ||
                InType.IsValueType;
        }
    }

    [ArgumentAttribute(100)]
    public class ArgumentDescriptionEnum : IArgumentDescription
    {
        public bool Accept(Type InType)
        {
            return InType != null && InType.IsEnum;
        }

        public string GetValue(Type InType, string InArgument)
        {
            DebugHelper.Assert(InArgument != null);

            string[] Enums = Enum.GetNames(InType);            

            for( int i=0; i<Enums.Length; ++i)
            {
                if( Enums[i].Equals(InArgument, StringComparison.CurrentCultureIgnoreCase) )
                {
                    return Enums[i];
                }
            }

            string DummyString;

            if (ArgumentDescriptionDefault.CheckConvertUtil(InArgument, typeof(int), out DummyString))
            {
                int EnumValue = System.Convert.ToInt32(InArgument);

                string Result = Enum.GetName(InType, EnumValue);

                return Result;
            }

            return "";
        }

        public bool CheckConvert(string InArgument, Type InType, out string OutErrorMessage)
        {
            DebugHelper.Assert(InArgument != null && InType.IsEnum);

            OutErrorMessage = "";

            string[] Enums = Enum.GetNames(InType);

            for (int i = 0; i < Enums.Length; ++i)
            {
                if (Enums[i].Equals(InArgument, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }

            string DummyString;

            if (ArgumentDescriptionDefault.CheckConvertUtil(InArgument, typeof(int), out DummyString))
            {
                int EnumValue = System.Convert.ToInt32(InArgument);

                string Result = Enum.GetName(InType, EnumValue);

                if( string.IsNullOrEmpty(Result) )
                {
                OutErrorMessage = string.Format("Failed Convert\"{0}\" to {1}.", InArgument, InType.Name);
                }
                
                return false;
            }

        OutErrorMessage = string.Format("Value \"{0}\" is not an valid property.", InArgument);

            return false;
        }

        public static int StringToEnum(Type InType, string InText)
        {
            // assume this input text always can be convert to enum.
            string DummyString;
            if (ArgumentDescriptionDefault.CheckConvertUtil(InText, typeof(int), out DummyString))
            {
                int EnumValue = System.Convert.ToInt32(InText);

                return EnumValue;
            }
            else
            {
                return System.Convert.ToInt32(Enum.Parse(InType, InText, true));
            }
        }

        public List<string> GetCandinates(Type InType)
        {
            string[] Results = Enum.GetNames(InType);

        return Results != null ? LinqS.ToStringList(Results) : null;
        }

        public List<string> FilteredCandinates(Type InType, string InArgument)
        {
            string DummyString;
            if (ArgumentDescriptionDefault.CheckConvertUtil(InArgument, typeof(int), out DummyString))
            {
                int EnumValue = System.Convert.ToInt32(InArgument);
                string CurrentString = Enum.GetName(InType, EnumValue);

                return FilteredCandinatesInner(InType, CurrentString);
            }
            else
            {
                return FilteredCandinatesInner(InType, InArgument);
            }
        }

        protected List<string> FilteredCandinatesInner(Type InType, string InArgument)
        {
            List<string> Results = GetCandinates(InType);

            if( Results != null && InArgument != null )
            {
                Results.RemoveAll((x) => !x.StartsWith(InArgument, StringComparison.CurrentCultureIgnoreCase));
            }

            return Results;
        }

        public bool AcceptAsMethodParameter(Type InType)
        {
            return InType.IsEnum;
        }

        public object Convert(string InArgument, Type InType)
        {
            string DummyString;
            if (ArgumentDescriptionDefault.CheckConvertUtil(InArgument, typeof(int), out DummyString))
            {
                int EnumValue = System.Convert.ToInt32(InArgument);

                return Enum.ToObject(InType, EnumValue);
            }
            else
            {
                return Enum.Parse(InType, InArgument, true);
            }
        }
    }
        
    public class ArgumentDescriptionRepository : Singleton<ArgumentDescriptionRepository>
    {
        private class Greater : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                return x > y ? -1 : (x==y?0:1);
            }
        }

        public SortedList<int, IArgumentDescription> Descriptions = new SortedList<int, IArgumentDescription>(new Greater());

        public ArgumentDescriptionRepository()
        {
        ClassEnumerator ArgDescEnumertor
            = new ClassEnumerator(
                typeof(ArgumentAttribute),
                typeof(IArgumentDescription),
                typeof(ArgumentAttribute).Assembly);

            var Iter = ArgDescEnumertor.results.GetEnumerator();

            while( Iter.MoveNext() )
            {
                Type ThisType = Iter.Current;

                var Attr = ThisType.GetCustomAttributes(typeof(ArgumentAttribute), false)[0] as ArgumentAttribute;

                IArgumentDescription DescInterface = Activator.CreateInstance(ThisType) as IArgumentDescription;

                Descriptions.Add(Attr.order, DescInterface);
            }
        }

        public IArgumentDescription GetDescription(Type InType)
        {
            var Iter = Descriptions.GetEnumerator();

            while( Iter.MoveNext() )
            {
                if( Iter.Current.Value.Accept(InType) )
                {
                    return Iter.Current.Value;
                }
            }

            DebugHelper.Assert(false, string.Format("can't find valid description for {0}, internal error!", InType.Name) );

            return null;
        }
    }
}
#endif
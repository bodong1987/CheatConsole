/**
 * @brief used to remove System.Linq
 * @email dbdongbo@vip.qq.com
*/
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Common
{
    public static class LinqS
    {
        public static List<string> ToStringList(string[] InStringArray)
        {
            if (InStringArray == null)
            {
                return null;
            }

            List<string> Items = new List<string>(InStringArray.Length);

            for (int i = 0; i < InStringArray.Length; ++i)
            {
                Items.Add(InStringArray[i]);
            }

            return Items;
        }

        public static string[] Skip(string[] InStringArray, int InCount)
        {
            if (InStringArray == null)
            {
                return null;
            }

            int Remain = InStringArray.Length - InCount;

            string[] Results = new string[Remain];

            for (int i = InCount; i < InStringArray.Length; ++i)
            {
                Results[i - InCount] = InStringArray[i];
            }

            return Results;
        }

        public static string[] Take(string[] InStringArray, int InCount)
        {
            if (InStringArray == null)
            {
                return null;
            }

            string[] Results = new string[InCount];

            for (int i = 0; i < InCount && i < InStringArray.Length; ++i)
            {
                Results[i] = InStringArray[i];
            }

            return Results;
        }

        public static string[] Where(string[] InStringArray, Func<string, bool> InPredicate)
        {
            if (InStringArray == null)
            {
                return null;
            }

            List<string> Results = new List<string>(InStringArray.Length);

            for (int i = 0; i < InStringArray.Length; ++i)
            {
                if (InPredicate(InStringArray[i]))
                {
                    Results.Add(InStringArray[i]);
                }
            }

            return Results.ToArray();
        }

        public static bool Contains<T>(T[] InArray, T InTest)
        {
            if (InArray == null || InArray.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < InArray.Length; ++i)
            {
                if (Comparer<T>.Equals(InArray[i], InTest))
                {
                    return true;
                }
            }

            return false;
        }

        public static T[] ToArray<T>(ListView<T> InList)
        {
            if (InList == null)
            {
                return null;
            }

            T[] Results = new T[InList.Count];

            for (int i = 0; i < InList.Count; ++i)
            {
                Results[i] = InList[i];
            }

            return Results;
        }
    }
}
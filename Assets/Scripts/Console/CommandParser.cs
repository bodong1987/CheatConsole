/**
 * @brief simple parser
 * @email dbdongbo@vip.qq.com
*/

#if !WITH_OUT_CHEAT_CONSOLE && (UNITY_STANDALONE || UNITY_EDITOR)

using System.Text.RegularExpressions;
using Assets.Scripts.Common;

namespace Assets.Scripts.Console
{
    public class CommandParser
    {
        public static readonly string RegexPattern = " (?=(?:[^\\\"]*\\\"[^\\\"]*\\\")*[^\\\"]*$)";

        public string text { get; protected set; }
        public string[] sections { get; protected set; }

        public string baseCommand { get; protected set; }

        public string[] arguments { get; protected set; }

        public CommandParser()
        {
            text = baseCommand = "";
        }

        public void Parse(string InText)
        {
            text = InText;

            sections = Regex.Split(text, RegexPattern);

            // sections = sections.Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();

            for (int i = 0; i < sections.Length; ++i)
            {
                sections[i] = sections[i].Trim('"');
            }

            if (sections.Length > 0)
            {
                baseCommand = sections[0];
            }
            else
            {
                baseCommand = "";
            }

            if (sections.Length > 1)
            {
                arguments = LinqS.Skip(sections, 1);
            }
            else
            {
                arguments = null;
            }
        }
    }
}
#endif
#if !WITH_OUT_CHEAT_CONSOLE
using System;
using Assets.Scripts.Common;

namespace Assets.Scripts.Console
{
	public enum TestEnum
	{
		AAA,
		BBB,
		CCC
	}
	
	[CheatCommandAttribute("Test/TestNormal", "DemoTestNormal")]
	[ArgumentDescriptionAttribute(0, typeof(TestEnum), "Attr")]
	[ArgumentDescriptionAttribute(1, typeof(int), "Value", 0, "AAA|CCC")]
	[ArgumentDescriptionAttribute(2, typeof(int), "Value2", 0, "AAA|CCC")]
	public class TestCommandNormal : CheatCommandCommon
	{
		protected override string Execute(string[] InArguments)
		{
			TestEnum EnumValue = (TestEnum)StringToEnum(InArguments[0], typeof(TestEnum));
			
			ConsoleWindow.instance.AddMessage(EnumValue.ToString());
						
			return Done;
		}
	}
	
	
	[CheatCommandEntryAttribute("Test")]
    public class TestCommandEntry
    {
        [CheatCommandEntryMethodAttribute("TestEntry1", false)]
        public static string TestEntry1( string InText )
        {
            ConsoleWindow.instance.AddMessage(InText);

            return CheatCommandBase.Done;
        }

        [CheatCommandEntryMethodAttribute("Tests/TestEntry2", false)]
        public static string TestEntry2( TestEnum InEnum )
        {
            ConsoleWindow.instance.AddMessage(((int)InEnum).ToString());

            return CheatCommandBase.Done;
        }
    }
	
}
#endif
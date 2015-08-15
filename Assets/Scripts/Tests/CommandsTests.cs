#if !WITH_OUT_CHEAT_CONSOLE

using Assets.Scripts.Common;

namespace Assets.Scripts.Console
{
    enum PlayerAttrType
    {
        Health,
        MaxHealth,
        Exp,
        MaxExp,
        Level
    }

    [CheatCommandAttribute("Player/Client/SetPlayerAttr", "Change Player Attribute Value.")]
    [ArgumentDescriptionAttribute(0, typeof(PlayerAttrType), "Attribute Type")]
    [ArgumentDescriptionAttribute(ArgumentDescriptionAttribute.EDefaultValueTag.Tag, 1, typeof(int), "Value", "2015")]
    public class SetPlayerAttrCommand : CheatCommandCommon
    {
        protected override string Execute(string[] InArguments)
        {
            if( ConsoleWindow.HasInstance() )
            {
                PlayerAttrType Type = (PlayerAttrType)StringToEnum(InArguments[0], typeof(PlayerAttrType));
                int Value = SmartConvert<int>(InArguments[1]);

                ConsoleWindow.instance.AddMessage(
                    string.Format("Set Player {0} to {1}", Type.ToString(), Value)
                    );

                return Done;
            }

            return "No Console Window";
        }
    }

    public abstract class NetworkMessageIDDefinition
    {
        public const int SetPlayerAttrID = 10;
    }

    public class SetPlayerAttrMessage : Networking.IMessage
    {
        public int AttrType;
        public int Value;

        public void Send()
        {
            if (ConsoleWindow.HasInstance())
            {
                PlayerAttrType Type = (PlayerAttrType)AttrType;                

                ConsoleWindow.instance.AddMessage(
                    string.Format("Server Set Player {0} to {1}", Type.ToString(), Value)
                    );
            }
        }
    }

    [CheatCommandAttribute("Player/Server/ServerSetPlayerAttr", "Change Player Attribute Value On Server", NetworkMessageIDDefinition.SetPlayerAttrID)]
    [ArgumentDescriptionAttribute(0, typeof(PlayerAttrType), "Attribute Type")]
    [ArgumentDescriptionAttribute(1, typeof(int), "Value")]
    public class ServerSetPlayerAttrCommand : Networking.NetworkingCheckCommand
    {
        protected override string ExecuteNetworking(string[] InArguments, Networking.IMessage InMessage)
        {
            var Message = (SetPlayerAttrMessage)InMessage;
            Message.AttrType = (int)(PlayerAttrType)StringToEnum(InArguments[0], typeof(PlayerAttrType));
            Message.Value = SmartConvert<int>(InArguments[1]);

            return Done;
        }

        protected override Networking.IMessage CreateMessage(int InMessageID)
        {
            return new SetPlayerAttrMessage();
        }
    }
}
#endif
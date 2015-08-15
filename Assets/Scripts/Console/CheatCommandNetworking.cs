/**
 * @brief Basic Networking Command root
 * @email bodong@tencent.com
*/

#if !WITH_OUT_CHEAT_CONSOLE
using Assets.Scripts.Common;
using System;
using System.Reflection;
using System.ComponentModel;

namespace Assets.Scripts.Console.Networking
{
    public interface IMessage
    {
        void Send();
    }

    public abstract class NetworkingCheckCommand : CheatCommandCommon
    {
        public override string StartProcess(string[] InArguments)
        {
            string ErrorMessage;

            if (!CheckArguments(InArguments, out ErrorMessage))
            {
                return ErrorMessage;
            }

            if (messageID != 0)
            {
                try
                {
                    IMessage Message = CreateMessage(messageID);

                    DebugHelper.Assert(Message != null);

                    string Result = ExecuteNetworking(InArguments, Message);
                    Message.Send();

                    return Result;
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            }
            else
            {
                return Execute(InArguments);
            }
        }

        protected abstract string ExecuteNetworking(string[] InArguments, IMessage InMessage);

        protected override string Execute(string[] InArguments)
        {
            throw new System.NotImplementedException(string.Format("Not Implement Execute For Type {0}", GetType().Name));
        }

        // @todo 
        // if you have your networking system
        // you can change this method to static function
        // and create message from your system by this message id.
        // this code here is just to explain how to create a simple network cheat command.
        protected virtual IMessage CreateMessage(int InMessageID)
        {
            return null;
        }
    }

}
#endif
/**
 * @brief History support/ only valid in PC Mode
 * @email bodong@tencent.com
*/

#if !WITH_OUT_CHEAT_CONSOLE && (UNITY_STANDALONE || UNITY_EDITOR)
// 
using System;
using System.Collections.Generic;
using System.Xml; // ignore
using System.Xml.Serialization;// ignore
using System.IO;
using Assets.Scripts.Common;

namespace Assets.Scripts.Console
{
    public class CommandLog
    {
        public string BaseCommand;
        public string FullCommand;

        public CommandLog()
        {
        }

        public CommandLog(string InBaseCommand, string InFullCommand)
        {
            BaseCommand = InBaseCommand;
            FullCommand = InFullCommand;
        }
    }

    [XmlRootAttribute("CommandHistory")]
    public class CommandHistory
    {
        [XmlArrayAttribute("Commands")]
        public List<CommandLog> Commands = new List<CommandLog>();

        [XmlIgnore]
        int Selection = -1;

        private string LogPath
        {
            get
            {
                return Path.Combine(UnityEngine.Application.dataPath, "../Commands.log.xml");
            }
        }

        protected virtual void CopyFrom(CommandHistory InOther)
        {
            Commands = InOther.Commands;
        }

        public void Load()
        {
            try
            {
                XmlSerializer Serializer = new XmlSerializer(GetType());
                using (FileStream fs = new FileStream(LogPath, FileMode.Open))
                {
                    CommandHistory tmp = Serializer.Deserialize(fs) as CommandHistory;

                    if (tmp != null)
                    {
                        CopyFrom(tmp);
                    }
                }
            }
            catch (Exception e)
            {
                DebugException(e);
            }
        }

        private void DebugException(Exception e)
        {
            DebugHelper.Assert(false, e.Message);
        }

        public void Save()
        {
            try
            {
                XmlSerializer Serializer = new XmlSerializer(GetType());
                using (TextWriter Writer = new StreamWriter(LogPath))
                {
                    Serializer.Serialize(Writer, this);
                }

            }
            catch (Exception e)
            {
                DebugException(e);
            }
        }

        public void PushCommand(String InBaseCommand, String InFullCommand)
        {
            for (int i = 0; i < Commands.Count; ++i)
            {
                if (Commands[i].BaseCommand == InBaseCommand)
                {
                    Commands.RemoveAt(i);
                    break;
                }
            }

            Commands.Add(new CommandLog(InBaseCommand, InFullCommand));
        }

        public void ResetSelection()
        {
            Selection = -1;
        }

        public CommandLog previousCommand
        {
            get
            {
                if (Selection > 0 && Selection < Commands.Count)
                {
                    return Commands[--Selection];
                }
                else if (Selection == -1 && Commands.Count > 0)
                {
                    Selection = Commands.Count - 1;
                    return Commands[Selection];
                }

                return null;
            }
        }

        public CommandLog nextCommand
        {
            get
            {
                if (Selection >= 0 && Selection < Commands.Count - 1)
                {
                    return Commands[++Selection];
                }

                return null;
            }
        }
    }
}
#endif
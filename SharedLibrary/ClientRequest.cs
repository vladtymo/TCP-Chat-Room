using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    [Serializable]
    public enum RequestCommand
    {
        Join, Leave, Message
    }
    [Serializable]
    public class ClientCommand
    {
        public string Nickname { get; set; }
        public RequestCommand Command { get; set; }
        public string Message { get; set; }

        public ClientCommand() { }
        public ClientCommand(RequestCommand cmd)
        {
            Command = cmd;
        }
        public ClientCommand(RequestCommand cmd, string nick)
        {
            Command = cmd;
            Nickname = nick;
        }
    }
}

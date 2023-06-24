using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Threading.Tasks;

namespace CharServer
{
    class Program
    {
        private const string localIP = "127.0.0.1";
        private const short localPort = 1234;

        static TcpListener server;

        static void Main(string[] args)
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(localIP), localPort);

            server = new TcpListener(localEndPoint);
            server.Start();

            Chat chat = new Chat();

            while (true)
            {
                Console.WriteLine("Waiting for a request...");
                TcpClient client = server.AcceptTcpClient();

                Console.WriteLine($"\tGot a TcpClient!");

                ClientCommand req = GetClientRequest(client);

                Console.WriteLine($"\tGot a {req.Command} request!");

                if (req.Command == RequestCommand.Join)
                {
                    chat.AddMember(client);
                }
            }
        }

        static ClientCommand GetClientRequest(TcpClient client)
        {
            //return JsonSerializer.DeserializeAsync<ClientRequest>(client.GetStream()).Result;
            //return Utils.Deserialize<ClientRequest>(client.GetStream());
            BinaryFormatter formatter = new BinaryFormatter();
            return (ClientCommand)formatter.Deserialize(client.GetStream());
        }
    }

    class Chat
    {
        List<TcpClient> members = new List<TcpClient>();

        public void AddMember(TcpClient client)
        {
            members.Add(client);
            Task.Run(() => ListenClient(client));
        }

        public void ListenClient(TcpClient client)
        {
            bool isExit = false;
            while (!isExit)
            {
                ClientCommand req = GetClientRequest(client);

                switch (req.Command)
                {
                    case RequestCommand.Leave:
                        members.Remove(client);
                        client.Close();
                        isExit = true;
                        break;
                    case RequestCommand.Message:
                        foreach (var m in members)
                        {
                            SendMessage(req.Message, m);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        void SendMessage(string msg, TcpClient reciever)
        {
            var cmd = new ClientCommand(RequestCommand.Message)
            {
                Message = msg
            };
            SendResponse(cmd, reciever);
        }
        private void SendResponse(ClientCommand cmd, TcpClient client)
        {
            NetworkStream ns = client.GetStream();

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ns, cmd);
            //await JsonSerializer.SerializeAsync(ns, req);
            ns.Flush();
        }
        static ClientCommand GetClientRequest(TcpClient client)
        {
            //return JsonSerializer.DeserializeAsync<ClientRequest>(client.GetStream()).Result;
            //return Utils.Deserialize<ClientRequest>(client.GetStream());
            BinaryFormatter formatter = new BinaryFormatter();
            return (ClientCommand)formatter.Deserialize(client.GetStream());
        }
    }
}

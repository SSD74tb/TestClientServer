using System.Net;
using System.Net.Sockets;
using System.Text;
using TCPServer.Authorizathion;
using Generic.History;

namespace tcpServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            const string ip = "127.0.0.1";
            const int port = 8080;
            IPEndPoint tcpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                tcpSocket.Bind(tcpEndPoint);
                tcpSocket.Listen();
                Console.WriteLine("Сервер запущен. Ожидание подключений... ");

                while (true)
                {
                    var tcpClient = await tcpSocket.AcceptAsync();

                    Task.Run(async () => await Host(tcpClient));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Auth.CheckDB();
        }

        static async Task Host(Socket socket)
        {
            Configuration clientConfig = new();
            while (true)
            {
                string firstMessage = Listener(socket);
                Console.WriteLine(firstMessage);

                if (firstMessage == "auth" || firstMessage == "reg")
                {
                    await Authorization(socket, firstMessage);
                }
                else if (firstMessage == "getcontent")
                {
                    GetContent(socket, clientConfig.N, clientConfig.L);
                }
                else if (firstMessage == "create")
                {
                    Auth.CheckDB();
                }
                else if (firstMessage == "close")
                {
                    Disconnect(socket);
                    break;
                }
                else if (firstMessage == "kurwabober")
                {
                    socket.Send(Encoding.UTF8.GetBytes("kurwaezhik"));
                }
                else if (firstMessage == "config")
                {
                    socket.Send(Encoding.UTF8.GetBytes("config"));
                    var tempConfig = Listener(socket).Split();
                    clientConfig.N = int.Parse(tempConfig[0]);
                    clientConfig.L = int.Parse(tempConfig[1]);
                    socket.Send(Encoding.UTF8.GetBytes("done"));
                }
                else
                {
                    socket.Send(Encoding.UTF8.GetBytes("cmd"));
                }
            }
        }

        static void GetContent(Socket socket, int configN, int configL)
        {
            socket.Send(Encoding.UTF8.GetBytes("start get content"));
            string[] story = Histoty.Speak(configN, configL);
            int i = 0;
            while (true)
            {
                if (i == story.Length)
                {
                    i = 0;
                    story = Histoty.Speak(configN, configL);
                }

                string firstMessage = Listener(socket);
                Console.WriteLine(firstMessage);

                if (firstMessage == "start")
                {
                    socket.Send(Encoding.UTF8.GetBytes(story[i]));
                    i++;
                }
                else if (firstMessage == "close")
                {
                    Disconnect(socket);
                    break;
                }
                else if (firstMessage == "stop")
                {
                    socket.Send(Encoding.UTF8.GetBytes("stop get content"));
                    break;
                }
                else
                {
                    socket.Send(Encoding.UTF8.GetBytes("idk this cmd"));
                }
            }
        }

        static async Task Authorization(Socket socket, string typeOperation)
        {
            socket.Send(Encoding.UTF8.GetBytes("1"));
            string userlogin = Listener(socket);
            socket.Send(Encoding.UTF8.GetBytes("1"));
            string password = Listener(socket);

            if (typeOperation == "auth")
            {
                await Auth.Login(socket, userlogin, password);
            }
            else await Auth.Registration(socket, userlogin, password);
        }

        static string Listener(Socket listener)
        {
            var buffer = new byte[256];
            int size;
            var data = new StringBuilder();

            do
            {
                size = listener.Receive(buffer); // получение данных, в size количество реально полученных байт
                data.Append(Encoding.UTF8.GetString(buffer, 0, size));
            } while (listener.Available > 0);

            return data.ToString();
        }

        private static void Disconnect(Socket socket)
        {
            socket.Send(Encoding.UTF8.GetBytes("close"));
            socket.Shutdown(SocketShutdown.Both);
        }

    }
}
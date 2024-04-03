using System.Net.Sockets;
using System.Text;
using TestClient;
using TCPClient.LogService;
using Serilog.Events;

namespace TCPClient
{
    class Programm
    {
        static void Main(string[] args)
        {
            Logger.CreateLogDirectory(
                LogEventLevel.Information,
                LogEventLevel.Warning,
                LogEventLevel.Error
            );

            TestFunction();

            

            while (true)
            {
                string? command = Console.ReadLine();
                Client.DialogWithServer(command);

                Logger.LogByTemplate(LogEventLevel.Information, note:$"Использована команда {command}");

                if (command == "close")
                {
                    Console.WriteLine("Соединение закрыто");
                    Logger.LogByTemplate(LogEventLevel.Information, note: $"Использована команда закрытия");
                    break;
                }
            }

            //for (int i = 0; i < 10; i++)
            //{
            //    Thread.Sleep(1000);
            //    Console.WriteLine($"Главный поток: {i}");
            //}

            SecondTest();

            try
            {
                string? command = Console.ReadLine();
                Client.DialogWithServer(command);
            }
            catch (SocketException ex)
            {
                Logger.LogByTemplate(LogEventLevel.Error, ex, "Произошла ошибка соединения с сервером.");
            }
            catch (Exception ex)
            {
                Logger.LogByTemplate(LogEventLevel.Error, ex, "Произошла ошибка в блоке кода.");
            }
            finally
            {
                Console.WriteLine("end.");
            }

            
        }

        static void TestFunction()
        {
            Client.Connect();
        }

        static void SecondTest()
        {
            Client.Disconnect();
        }
    }



    class Client
    {
        static Socket tcpClient = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public static void Connect()
        {
            try
            {
                tcpClient.Connect("127.0.0.1", 8080);
                Logger.LogByTemplate(LogEventLevel.Information, note: "Сервер запущен");
            }
            catch (Exception ex)
            {
                Logger.LogByTemplate(LogEventLevel.Error, ex, "Ошибка подключения к серверу");
            }
        }
        public static void DialogWithServer(string command="cmd")
        {
            tcpClient.Send(Encoding.UTF8.GetBytes(command));
            string answer = Listener(tcpClient);
            if (answer.Split()[0] == "Компания") Console.Clear();
            Console.WriteLine(answer);
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

        public static void Disconnect()
        {
            try
            {
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.ServiceProcess;
using System.Net.NetworkInformation;

namespace ServerApplication
{
    class Program
    {
        private static int connectedClients = 0;
        private static object clientLock = new object();

        public enum SimpleServiceCustomCommands
        { StopWorker = 128, RestartWorker, CheckWorker };
        public static async Task Main(string[] args)
        {
            string wirelessIp = null;
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface networkInterface in interfaces)
            {
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 &&
                    networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                    foreach (UnicastIPAddressInformation ipAddres in ipProperties.UnicastAddresses)
                    {
                        if (ipAddres.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            wirelessIp = ipAddres.Address.ToString();
                            break;
                        }
                    }
                }
            }

            IPAddress.TryParse(wirelessIp, out IPAddress iPAddress);
            IPEndPoint ipEndPoint = new(iPAddress, 7777);
            TcpListener server = new TcpListener(ipEndPoint);

            server.Start();
            Console.WriteLine("The server is running \nWaiting for connections...");

            Console.WriteLine($"Ip with port: {server.LocalEndpoint}");
            while (true)
            {
                // ожидание подключения клиента
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Клиент подключился!");

                lock (clientLock)
                {
                    if (connectedClients >= 4)
                    {
                        Console.WriteLine("Достигнуто максимальное число подключений.");
                        client.Close();
                        continue;
                    }

                    connectedClients++;
                    //PrintConnectedClientsCount();
                }

                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
        }

        static void HandleClient(object obj)
        {
            int id = connectedClients;
            ServiceController[] scServices;
            scServices = ServiceController.GetServices();

            TcpClient client = (TcpClient)obj;
            while (client.Connected)
            {
                NetworkStream stream = client.GetStream();

                byte[] data = new byte[1024];
                StringBuilder builder = new StringBuilder();

                int bytes = 0;

                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                } while (stream.DataAvailable);

                string message = builder.ToString();

                if (message == "1")
                {
                    string response = "";

                    foreach (ServiceController service in scServices)
                    {
                        ServiceController sc = new ServiceController(service.ServiceName);
                        response += sc.ServiceName + " " + sc.Status + "\n";
                    }

                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                }
                else if (message == "2")
                {
                    string response = "Enter service name: ";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                    responseData = Encoding.UTF8.GetBytes(response);
                    stream = client.GetStream();

                    data = new byte[1024];
                    builder = new StringBuilder();

                    bytes = 0;

                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    } while (stream.DataAvailable);

                    message = builder.ToString();
                    bool canDo = false;
                    ServiceController thisService = null;
                    bool isFind = false;
                    foreach (ServiceController service in scServices)
                    {
                        response = "";
                        if (message == service.ServiceName)
                        {
                            isFind = true;
                            thisService = new ServiceController(service.ServiceName);
                            response += thisService.ServiceName;
                            response += " " + thisService.Status;
                            if (thisService.CanPauseAndContinue && thisService.Status == ServiceControllerStatus.Running)
                            {
                                response += "\n/pause";
                                canDo = true;
                            }
                            if (thisService.CanPauseAndContinue && thisService.Status == ServiceControllerStatus.Paused)
                            {
                                response += "\n/continue";
                                canDo = true;
                            }
                            if (thisService.Status == ServiceControllerStatus.Stopped)
                            {
                                response += "\n/start";
                                canDo = true;
                            }
                            if (thisService.CanStop)
                            {
                                response += "\n/stop";
                                canDo = true;
                            }
                            if (canDo)
                            {
                                response += "\n/exit";
                                response += "\nEnter Command:";
                            }
                            else
                            {
                                response += "\nYou can do nothing with this service!";
                            }
                            responseData = Encoding.UTF8.GetBytes(response);
                            stream.Write(responseData, 0, responseData.Length);
                            Console.WriteLine("Client" + id + " choose " + message);
                            break;
                        }
                    }
                    if (!isFind)
                    {
                        response += "Service not found!";
                        responseData = Encoding.UTF8.GetBytes(response);
                        stream.Write(responseData, 0, responseData.Length);
                    }
                    else if (canDo)
                    {
                        stream = client.GetStream();

                        data = new byte[1024];
                        builder = new StringBuilder();

                        bytes = 0;

                        do
                        {
                            bytes = stream.Read(data, 0, data.Length);
                            builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                        } while (stream.DataAvailable);

                        message = builder.ToString();
                        response = "";
                        if (message == "stop" && thisService.CanStop)
                        {
                            thisService.Stop();
                            response += "Service " + thisService.ServiceName + " was stopped!";
                            Console.WriteLine(response);
                            responseData = Encoding.UTF8.GetBytes(response);
                            stream.Write(responseData, 0, responseData.Length);
                        }
                        else if (message == "continue" && thisService.CanPauseAndContinue)
                        {
                            thisService.Continue();
                            response += "Service " + thisService.ServiceName + " was continued!";
                            Console.WriteLine(response);
                            responseData = Encoding.UTF8.GetBytes(response);
                            stream.Write(responseData, 0, responseData.Length);
                        }
                        else if (message == "pause" && thisService.CanPauseAndContinue)
                        {
                            thisService.Pause();
                            response += "Service " + thisService.ServiceName + " was continued!";
                            Console.WriteLine(response);
                            responseData = Encoding.UTF8.GetBytes(response);
                            stream.Write(responseData, 0, responseData.Length);
                        }
                        else if (message == "start" && thisService.Status == ServiceControllerStatus.Stopped)
                        {
                            thisService.Start();
                            response += "Service " + thisService.ServiceName + " was started!";
                            Console.WriteLine(response);
                            responseData = Encoding.UTF8.GetBytes(response);
                            stream.Write(responseData, 0, responseData.Length);
                        }
                        else if (message == "exit")
                        {
                            Console.WriteLine("Client returned to the menu");
                        }
                        else
                        {
                            response += "Invalid command! Just try again";
                            responseData = Encoding.UTF8.GetBytes(response);
                            stream.Write(responseData, 0, responseData.Length);
                        }
                    }
                }
                else if (message == "0")
                {
                    Console.WriteLine("Client was diconnected");
                    connectedClients--;
                    client.Close();
                }
                // Закрываем соединение с клиентом
            }
        }
    }
}
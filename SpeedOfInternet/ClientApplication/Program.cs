using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace ClientApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            string tempIp;
            Console.Write("Enter server IP Address ");
            tempIp = Console.ReadLine();
            TcpClient client = new TcpClient(tempIp, 7777);
            while (true)
            {
                // Подключаемся к серверу
                NetworkStream stream = client.GetStream();
                
                Console.WriteLine("Command list: ");
                Console.WriteLine("/1 Print services list");
                Console.WriteLine("/2 Choose service");
                Console.WriteLine("/0 Exit");
                Console.Write("Enter command: ");
                string request = Console.ReadLine();

                if (request == "1")
                {
                    Console.Clear();
                    byte[] requestData = Encoding.UTF8.GetBytes(request);
                    stream.Write(requestData, 0, requestData.Length);

                    byte[] responseData = new byte[256];
                    StringBuilder builder = new StringBuilder();

                    int bytes = 0;

                    do
                    {
                        bytes = stream.Read(responseData, 0, responseData.Length);
                        builder.Append(Encoding.UTF8.GetString(responseData, 0, bytes));
                    } while (stream.DataAvailable);

                    string response = builder.ToString();

                    
                    Console.WriteLine("Services List:");

                    foreach (string line in response.Split('\n'))
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            Console.WriteLine(line);
                        }
                    }
                }
                else if (request == "2")
                {
                    Console.Clear();
                    byte[] requestData = Encoding.UTF8.GetBytes(request);
                    stream.Write(requestData, 0, requestData.Length);

                    byte[] responseData = new byte[256];
                    StringBuilder builder = new StringBuilder();

                    int bytes = 0;

                    do
                    {
                        bytes = stream.Read(responseData, 0, responseData.Length);
                        builder.Append(Encoding.UTF8.GetString(responseData, 0, bytes));
                    } while (stream.DataAvailable);

                    string response = builder.ToString();
                    Console.WriteLine(response);
                    request = Console.ReadLine();
                    requestData = Encoding.UTF8.GetBytes(request);
                    stream.Write(requestData, 0, requestData.Length);

                    responseData = new byte[256];
                    builder = new StringBuilder();

                    bytes = 0;

                    do
                    {
                        bytes = stream.Read(responseData, 0, responseData.Length);
                        builder.Append(Encoding.UTF8.GetString(responseData, 0, bytes));
                    } while (stream.DataAvailable);

                    response = builder.ToString();
                    Console.WriteLine(response);
                    string[] parts = response.Split("\n");
                    if (parts.Length > 1)
                    {
                        if (parts[1] != "You can do nothing with this service!")
                        {
                            while (true)
                            {
                                request = Console.ReadLine();
                                requestData = Encoding.UTF8.GetBytes(request);
                                stream.Write(requestData, 0, requestData.Length);
                                if (request == "exit")
                                {
                                    break;
                                }
                                responseData = new byte[256];
                                builder = new StringBuilder();

                                bytes = 0;

                                do
                                {
                                    bytes = stream.Read(responseData, 0, responseData.Length);
                                    builder.Append(Encoding.UTF8.GetString(responseData, 0, bytes));
                                } while (stream.DataAvailable);

                                response = builder.ToString();
                                Console.WriteLine(response);
                                break;
                            }
                        }
                    }
                }
                else if (request == "0")
                {
                    byte[] requestData = Encoding.UTF8.GetBytes(request);
                    stream.Write(requestData, 0, requestData.Length);
                    client.Close();
                    break;
                }
            }
        }
    }
}
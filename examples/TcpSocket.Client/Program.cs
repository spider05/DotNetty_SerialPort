﻿using Coldairarrow.DotNettySocket;
using System;
using System.Text;
using System.Threading.Tasks;

namespace TcpSocket.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var theClient = await SocketBuilderFactory.GetTcpSocketClientBuilder("127.0.0.1", 6001)
                .SetLengthFieldEncoder(2)
                .SetLengthFieldDecoder(ushort.MaxValue, 0, 2, 0, 2)
                .OnClientStarted(client =>
                {
                    Console.WriteLine($"客户端启动");
                })
                .OnClientClose(client =>
                {
                    Console.WriteLine($"客户端关闭");
                })
                .OnException(ex =>
                {
                    Console.WriteLine($"异常:{ex.Message}");
                })
                .OnRecieve((client, bytes) =>
                {
                    Console.WriteLine($"客户端:收到数据:{Encoding.UTF8.GetString(bytes)}");
                })
                .OnSend((client, bytes) =>
                {
                    Console.WriteLine($"客户端:发送数据:{Encoding.UTF8.GetString(bytes)}");
                })
                .BuildAsync();
            bool flag = true;
          
            while (flag)
            {
                string str = Console.ReadLine();
                if (str == "ok")
                {
                    //await theClient.Send(Guid.NewGuid().ToString());

                  byte[] bytes=  BitConverter.GetBytes(2);

                    await theClient.Send(bytes);

                    await Task.Delay(1000);

                }
                else if (str == "exit")
                {
                    flag = false;
                }
                str = Console.ReadLine();
            }
                
           
        }
    }
}
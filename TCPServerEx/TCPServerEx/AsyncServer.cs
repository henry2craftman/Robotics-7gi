using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPServerEx
{
    internal class AsyncServer
    {
        static async Task Main()
        {
            // 1. 서버 시작
            TcpListener server = new TcpListener(IPAddress.Any, 12345);
            server.Start();
            Console.WriteLine("서버가 시작되었습니다.");

            while(true)
            {
                // 2. 클라이언트 연결 -> 비동기
                await ResponseAsync(server);
            }
        }

        static async Task ResponseAsync(TcpListener _server)
        {
            TcpClient client = await _server.AcceptTcpClientAsync();
            Console.WriteLine("클라이언트가 연결되었습니다.");

            // 3. 새 쓰레드가 클라이언트의 요청을 처리
            // Task.Factory.StartNew(ProcessAsync, client);
            await Task.Run(() => ProcessAsync(client));
        }

        static async void ProcessAsync(object o)
        {
            while (true)
            {
                try
                {
                    TcpClient client = (TcpClient)o;

                    NetworkStream stream = client.GetStream();

                    // 4. 클라이언트 세션의 스트림 읽기
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    Console.WriteLine(data);

                    if (data.Contains("exit"))
                    {
                        client.Close();
                        break;
                    }

                    // 5. Echo 보내기
                    string echo = "Hello from server!";
                    byte[] echoBytes = Encoding.UTF8.GetBytes(echo);
                    await stream.WriteAsync(buffer, 0, bytesRead);

                    // 6. 클라이언트 세션 해지
                    //client.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    break;
                }
            }
        }
    }
}

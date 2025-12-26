using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;

namespace TCPClient
{
    class AsyncClient
    {
        static async Task Main()
        {
            // 현재 내 아이피 정보 가져오기
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            string myIp = "";
            foreach(var ip in host.AddressList)
            {
                if(ip.AddressFamily == AddressFamily.InterNetwork)
                    myIp = ip.ToString();
            }


            // 1. 클라이언스 생성 및 비동기 연결시도
            TcpClient client = new TcpClient();
            await client.ConnectAsync("192.168.10.95", 12345);

            Console.WriteLine("서버에 연결되었습니다. " + myIp);
            Console.WriteLine("메시지를 입력하고 Enter 키를 눌러주세요.('exit' 입력 시 종료");

            // 2. 서버 통신을 위한 스트림열기
            NetworkStream stream = client.GetStream();

            // 3. 반복 메시지 송수신
            while (true)
            {
                Console.Write("보낼 메시지: ");
                string message = Console.ReadLine();

                if (message.Equals("exit"))
                {
                    byte[] endData = Encoding.UTF8.GetBytes($"{myIp}: exit");
                    await stream.WriteAsync(endData, 0, endData.Length);

                    break;
                }

                // 4. 메시지를 바이트 배열로 변환하여 전송
                byte[] data = Encoding.UTF8.GetBytes($"{myIp}: " + message);
                await stream.WriteAsync(data, 0, data.Length);

                // 5. 서버로 부터 받은 응답 수신
                byte[] buffer = new byte[1024];
                int byteRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, byteRead);

                Console.WriteLine($"서버 응답: {response}");
            }
        }
    }
}
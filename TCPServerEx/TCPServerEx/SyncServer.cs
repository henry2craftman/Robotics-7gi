using System.Net;
using System.Net.Sockets;
using System.Text;

class SyncServer
{
    static void Main23()
    {
        // 1. 서버 소켓 생성 및 바인딩(어떤 IP에서든 12345포트를 사용하면 접속 가능)
        TcpListener server = new TcpListener(IPAddress.Any, 12345);
        
        // 2. 서버 시작
        server.Start();

        Console.WriteLine("서버 시작");

        while (true)
        {
            // 3. 클라이언트 연결 대기
            TcpClient client = server.AcceptTcpClient();

            Console.WriteLine("클라이언트 연결됨");

            // 4. 클라이언트의 네트워크 스트림 가져오기
            NetworkStream stream = client.GetStream();

            // 5. 클라이언트의 스트림으로 부터 데이터 읽기
            byte[] buffer = new byte[1024];
            int byteRead = stream.Read(buffer, 0, buffer.Length);
            string data = Encoding.UTF8.GetString(buffer, 0, byteRead);

            Console.WriteLine("클라이언트: " + data);

            // 6. 클라이언트에 데이터 보내기
            string response = "Hello from Server!";
            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            stream.Write(responseBytes, 0, responseBytes.Length);

            // 7. 클라이언트와의 세션을 종료하고 리소스를 해제
            client.Close();
        }
    }
}
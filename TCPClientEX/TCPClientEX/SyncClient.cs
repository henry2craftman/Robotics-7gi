using System.Net.Sockets;
using System.Text;

class SyncClient
{
    static void Main2()
    {
        // 1. 서버에 연동
        TcpClient client = new TcpClient("127.0.0.1", 12345);

        Console.WriteLine("서버에 연결됨");

        // 2. 네트워크 스트림 얻기
        NetworkStream stream = client.GetStream();

        // 3. 스트림에 데이터 쓰기
        string data = "Hello from client!";
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        stream.Write(dataBytes, 0, dataBytes.Length);

        // 4. (서버가 보낸)스트림 데이터 읽기
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine(response);

        // 5. 연결 종료(클라이언트 세션 종료, 리소스 해제)
        client.Close();
    }
}
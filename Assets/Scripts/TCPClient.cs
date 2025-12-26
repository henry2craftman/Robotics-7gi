using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/// <summary>
/// 서버에 접속해서 현재 나의 위치, 회전정보를 송신한다.
/// 다른 사람의 위치, 회전정보를 수신한다.
/// 속성: 서버IP, 포트번호, 정보전송주기
/// 기능: 서버연결, 연결해지, 내아이피확인
/// </summary>
public class TCPClient : MonoBehaviour
{
    public Transform player;

    public bool isConnected = false;
    public string serverIP = "192.168.0.95";
    public string myIP = "";
    public int port = 12345;
    public int updateInterval = 1;
    public string data;

    TcpClient client;
    NetworkStream stream;
    byte[] buffer;

    private void Start()
    {
        GetIP();
    }

    public void GetIP()
    {
        // 현재 내 아이피 정보 가져오기
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                myIP = ip.ToString();
        }
    }

    public void ConnectToServer()
    {
        try
        {
            client = new TcpClient(serverIP, port);
            stream = client.GetStream();

            isConnected = true;

            StartCoroutine(CoUpdateData());
            Debug.Log("서버에 연결되었습니다.");
        }
        catch (Exception e)
        {
            isConnected = false;

            Debug.LogError($"Exception: {e.Message}");
        }
    }

    IEnumerator CoUpdateData()
    {
        while (isConnected)
        {
            try
            {
                // 보낼 데이터
                // 192.168.0.95/pos:37,60,180/rot:30,20,15
                data = $"{myIP}/" +
                        $"pos:{player.position.x},{player.position.y},{player.position.z}/" +
                        $"rot:{player.rotation.x},{player.rotation.y},{player.rotation.x},{player.rotation.z}";
                buffer = Encoding.UTF8.GetBytes(data);
                stream.Write(buffer, 0, buffer.Length);

                // 받은 데이터
                byte[] buffer2 = new byte[1024];
                int byteRead = stream.Read(buffer2, 0, buffer2.Length);
                string reponse = Encoding.UTF8.GetString(buffer2, 0, byteRead);
            }
            catch(Exception e)
            {
                Debug.LogError($"Error: {e.Message}");
                
                break;
            }

            yield return new WaitForSeconds(updateInterval);
        }

        try
        {
            buffer = Encoding.UTF8.GetBytes($"{myIP} exit");
            stream.Write(buffer, 0, buffer.Length);
        }
        catch(Exception e)
        {
            Debug.LogError($"Error: {e.Message}");
        }
    }

    public void OnDisconnectBtnClkEvent()
    {
        isConnected = false;
    }
}

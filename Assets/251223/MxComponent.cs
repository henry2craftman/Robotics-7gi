using UnityEngine;
using ActUtlType64Lib;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Threading;
using Unity.VisualScripting;

/// <summary>
/// MxComponent 객체를 사용하여 PLC와 통신한다
/// - 기능: Open, Close, ReadDeviceBlock, WriteDeviceBlock
/// - 속성: mxComponent 변수, interval
/// - Data정의(* Data: PLC에서 읽은 블록 형태의 데이터, { 255, 55, 30 })
/// yData = 2047 = 0000011111111111(b)
/// </summary>
public class MxComponent : MonoBehaviour
{
    public static MxComponent instance; // 싱글턴패턴: 한 씬에 하나의 객체를 공지
    ActUtlType64 mxComponent;
    public bool isConnected = false;
    public float interval = 1;
    //bool[,] data = new bool[3,16];
    public List<bool[]> plcData = new List<bool[]>(); // 공용 데이터 컨테이너
    public int[] plcXData = new int[1];               // PLCManager에서 전달받은 정보
    CancellationTokenSource cts = new CancellationTokenSource();    // 스레드 관리자!

    private void Awake()
    {
        // 싱글턴 객체 할당
        if (instance == null)
            instance = this;
    }

    /// <summary>
    /// 메인스레드만 사용시 PLCManager.cs의 Connect버튼과 연결
    /// </summary>
    /// <returns></returns>
    public int Open()
    {
        if (isConnected) return -1;

        if(mxComponent == null)
            mxComponent = new ActUtlType64(); // 메인스레드에서 mxComponent 사용시

        int iRet = mxComponent.Open();

        if(iRet == 0)
        {
            isConnected = true;

            StartCoroutine(UpdatePLCData());
        }

        return iRet;
    }

    /// <summary>
    /// PLC와 통신시 발생하는 블로킹 현상을 방지하기 위해 새로운 워커스레드 사용
    /// PLCManager.cs의 버튼 UI 메소드와 연결
    /// </summary>
    public void OpenAsync()
    {
        Task.Run(UpdatePLCDataAsync, cts.Token);
    }

    /// <summary>
    /// 메인스레드만 사용시 PLCManager.cs의 Disconnect버튼과 연결
    /// </summary>
    /// <returns></returns>
    public int Close()
    {
        if (!isConnected) return -1;

        int iRet = mxComponent.Close();

        if (iRet == 0)
            isConnected = false;

        return iRet;
    }

    /// <summary>
    /// 다중 스레드 사용시 PLCManager.cs의 Disconnect버튼과 연결
    /// </summary>
    public void CloseAsync()
    {
        isConnected = false;
    }

    /// <summary>
    /// 유니티 Main 스레드에서 작동시키는 방법 -> Blocking 문제 발생 -> 40~80 FPS 
    /// </summary>
    /// <returns></returns>
    public IEnumerator UpdatePLCData()
    {
        while(isConnected)
        {
            // 디바이스의 시작주소와, 블록의 개수를 입력
            ReadDeviceBlock("Y0", 1);

            WriteDeviceBlock("X0", 1, ref plcXData);

            yield return new WaitForSeconds(interval);
        }
    }

    async void UpdatePLCDataAsync()
    {
        mxComponent = new ActUtlType64();

        int iRet = mxComponent.Open();

        if(iRet == 0)
        {
            isConnected = true;
            Debug.Log("PLC와 연결이 성공적으로 완료되었습니다!");
        }
        else
        {
            Debug.LogWarning(iRet.ToString("X"));
        }

        while (isConnected)
        {
            ReadDeviceBlock(mxComponent, "Y0", 1);

            WriteDeviceBlock(mxComponent, "X0", 1, ref plcXData);

            // 1f, 0.3f -> 1000ms, 300ms
            int newInterval = Convert.ToInt32(interval * 1000);

            await Task.Delay(newInterval, cts.Token);
        }

        iRet = mxComponent.Close();

        if(iRet == 0)
        {
            Debug.Log("PLC와 연결이 해지되었습니다!");
        }
        else
        {
            Debug.Log(iRet.ToString("X"));
        }
    }

    private void OnDestroy()
    {
        if(cts != null)
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    public void ReadDeviceBlock(string startDevice, int blockNum)
    {
        if(isConnected)
        {
            int[] data = new int[blockNum];
            int iRet = mxComponent.ReadDeviceBlock(startDevice, blockNum, out data[0]);

            if(iRet == 0)
            {
                // 데이터 변환(가상의 설비들이 잘 사용할 수 있도록 하기 위해)
                print(data[0]);
                // 1023 -> { true, false, true, false,  true, false, true, false, true, false, true, false,  true, false, true, false }
                plcData = ConvertDecimalToBinary(data);
            }
            else
            {
                Debug.LogWarning(iRet.ToString("X"));
            }
        }
        else
        {
            Debug.LogWarning("PLC와 연결이 되어있지 않습니다.");
        }
    }

    public void ReadDeviceBlock(ActUtlType64 _mxComp, string startDevice, int blockNum)
    {
        if (isConnected)
        {
            int[] data = new int[blockNum];
            int iRet = _mxComp.ReadDeviceBlock(startDevice, blockNum, out data[0]);

            if (iRet == 0)
            {
                // 데이터 변환(가상의 설비들이 잘 사용할 수 있도록 하기 위해)
                print(data[0]);
                // 1023 -> { true, false, true, false,  true, false, true, false, true, false, true, false,  true, false, true, false }
                plcData = ConvertDecimalToBinary(data);
            }
            else
            {
                Debug.LogWarning(iRet.ToString("X"));
            }
        }
        else
        {
            Debug.LogWarning("PLC와 연결이 되어있지 않습니다.");
        }
    }

    /// <summary>
    /// PLC로 부터 받은 10진수 배열(블록) 데이터 {2047, 1244, 25}
    /// 를 2진수 형태의 bool 배열로 변환 
    /// { true, false, true, false ... 총 16개 }
    /// { true, false, true, false ... 총 16개 }
    /// { true, false, true, false ... 총 16개 }
    /// "0000111111111111"
    /// </summary>
    /// <param name="data">PLC로 부터 받은 10진수 배열(블록) 데이터</param>
    /// <returns></returns>
    private List<bool[]> ConvertDecimalToBinary(int[] data)
    {
        List<bool[]> result = new List<bool[]>();

        // 1. 받은 블록 데이터 순회
        for (int i = 0; i < data.Length; i++)
        {
            bool[] block = new bool[16]; // 2진수 임시저장공간

            // data[0] = 2047 = (b)0000011111111111
            //                                  1
            //                                  1
            for(int j = 0;  j < block.Length; j++)
            {
                bool isBitSet = ((data[i] & (1 << j)) != 0);
                block[j] = isBitSet;
            }
            // block = { t, t, t, t, t, t, t, t, t, t, t, f,f,f,f,f}
            // cylinder1.forwardSignal  = block[0];
            // cylinder1.backwardSignal = block[1];

            result.Add(block);
        }

        return result;
    }

    /// <summary>
    /// MPS 설비의 INPUT(X0...) 신호를 PLC로 전달해 준다.
    /// * 참고: PLC에 연동된 센서가 없기 때문에, PLC프로그램을 작동을 위한 가상 Sensor를 하드웨어 센서처럼 사용
    /// </summary>
    /// <param name="startDevice">쓰기 위한 블록의 시작 디바이스 주소</param>
    /// <param name="blockNum">쓰기위한 블록의 개수</param>
    /// <param name="data">MPS의 Sensor들의 정보를 넣어주기 위한 인자</param>
    public void WriteDeviceBlock(string startDevice, int blockNum, ref int[] data)
    {
        if(isConnected)
        {
            int iRet = mxComponent.WriteDeviceBlock(startDevice, blockNum, ref data[0]);

            if(iRet == 0)
            {
                Debug.Log("쓰기가 완료되었습니다.");
            }
            else
            {
                Debug.LogWarning(iRet.ToString("X"));
            }
        }
        else
        {
            Debug.LogWarning("PLC와 연결이 되어있지 않습니다.");
        }
    }

    public void WriteDeviceBlock(ActUtlType64 _mxComp, string startDevice, int blockNum, ref int[] data)
    {
        if (isConnected)
        {
            int iRet = _mxComp.WriteDeviceBlock(startDevice, blockNum, ref data[0]);

            if (iRet == 0)
            {
                Debug.Log("쓰기가 완료되었습니다.");
            }
            else
            {
                Debug.LogWarning(iRet.ToString("X"));
            }
        }
        else
        {
            Debug.LogWarning("PLC와 연결이 되어있지 않습니다.");
        }
    }
}

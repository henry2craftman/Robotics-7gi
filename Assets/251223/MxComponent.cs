using UnityEngine;
using ActUtlType64Lib;
using System.Collections;
using System.Collections.Generic;
using System;

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

    private void Awake()
    {
        // 싱글턴 객체 할당
        if (instance == null)
            instance = this;
    }

    public int Open()
    {
        if (isConnected) return -1;

        if(mxComponent == null)
            mxComponent = new ActUtlType64();

        int iRet = mxComponent.Open();

        if(iRet == 0)
        {
            isConnected = true;

            StartCoroutine(UpdatePLCData());
        }

        return iRet;
    }

    public int Close()
    {
        if (!isConnected) return -1;

        int iRet = mxComponent.Close();

        if (iRet == 0)
            isConnected = false;

        return iRet;
    }

    public IEnumerator UpdatePLCData()
    {
        while(isConnected)
        {
            ReadDeviceBlock("Y0", 1);

            yield return new WaitForSeconds(interval);
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

    /// <summary>
    /// PLC로 부터 받은 10진수 배열(블록) 데이터 {2047, 1244, 25}
    /// 를 2진수 형태의 bool 배열로 변환 
    /// { true, false, true, false ... 총 16개 }
    /// { true, false, true, false ... 총 16개 }
    /// { true, false, true, false ... 총 16개 }
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
                bool isBitSet = ((data[i] & (1 << j)) == 1);
                block[j] = true;
            }
            // block = { t, t, t, t, t, t, t, t, t, t, t, f,f,f,f,f}
            // cylinder1.forwardSignal  = block[0];
            // cylinder1.backwardSignal = block[1];
            // cylinder2.backwardSignal = block[2];
            // cylinder3.backwardSignal = block[3];

            result.Add(block);
        }

        return result;
    }

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
}

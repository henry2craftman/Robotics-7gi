using MPSSimulator;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UI;

/// <summary>
/// 버튼들을 눌렀을 때, PLC로 신호가 전달된다.
/// 속성: 버튼들
/// </summary>
public class PLCManager : MonoBehaviour
{
    [Header("PLC 신호")]
    public bool startSignal;
    public bool stopSignal;

    [Header("장비 세팅")]
    [SerializeField] Cylinder cylinder1;    // 양솔
    [SerializeField] Cylinder cylinder2;    // 단솔
    [SerializeField] Cylinder cylinder3;    // 단솔
    [SerializeField] Cylinder cylinder4;    // 단솔
    [SerializeField] TowerLamp towerLamp;
    [SerializeField] Conveyor conveyor;
    [SerializeField] Loader loader;
    [SerializeField] MPSSimulator.Sensor loaderSensor;
    [SerializeField] MPSSimulator.Sensor metalSensor;
    [SerializeField] MPSSimulator.Sensor proximitySensor;

    [Header("UI 버튼 세팅")]
    [SerializeField] Button 시작버튼;
    [SerializeField] Button 스탑버튼;
    [SerializeField] Button redLamp버튼;
    [SerializeField] Button yellowLamp버튼;
    [SerializeField] Button greenLamp버튼;
    [SerializeField] Button convCW버튼;
    [SerializeField] Button convCCW버튼;
    [SerializeField] Button loader버튼;
    [SerializeField] Button connect버튼;
    [SerializeField] Button disConnect버튼;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        시작버튼.onClick.AddListener(OnStartBtnClkEvent);
        스탑버튼.onClick.AddListener(OnStopBtnClkEvent);
        redLamp버튼.onClick.AddListener(OnRedLampBtnClkEvent);
        yellowLamp버튼.onClick.AddListener(OnYellowLampBtnClkEvent);
        greenLamp버튼.onClick.AddListener(OnGreenLampBtnClkEvent);
        convCW버튼.onClick.AddListener(OnConvCWBtnClkEvent);
        convCCW버튼.onClick.AddListener(OnConvCCWBtnClkEvent);
        loader버튼.onClick.AddListener(OnLoaderBtnClkEvent);
        connect버튼.onClick.AddListener(OnConnectBtnClkEvent);
        disConnect버튼.onClick.AddListener(OnDisconnectBtnClkEvent);
    }

    private void Update()
    {
        if (!MxComponent.instance.isConnected) return;

        // cylinder1은 양솔                            //[블록번호][디바이스 번호]
        cylinder1.forwardSignal  = MxComponent.instance.plcData[0][0];
        cylinder1.backwardSignal = MxComponent.instance.plcData[0][1];

        // 나머지는 단동형
        cylinder2.forwardSignal  = MxComponent.instance.plcData[0][2];
        cylinder3.forwardSignal  = MxComponent.instance.plcData[0][3];
        cylinder4.forwardSignal  = MxComponent.instance.plcData[0][4];

        // 타워램프
        towerLamp.redLamSignal   = MxComponent.instance.plcData[0][5];
        towerLamp.yellowLamSignal= MxComponent.instance.plcData[0][6];
        towerLamp.greenLamSignal = MxComponent.instance.plcData[0][7];

        // Loader
        loader.loadSignal        = MxComponent.instance.plcData[0][8];

        // 컨베이어
        conveyor.cwSignal        = MxComponent.instance.plcData[0][9];
        conveyor.ccwSignal       = MxComponent.instance.plcData[0][10];    // 0A

        // 가상의 센서정보의 순서정의 실린더LS0 -> X00, 실린더1LS1 -> X01
        //  X0 X1 X2 ....
        // "10101010100" -> "00101010101" -> 341

        int ls1 = cylinder1.lsBackwardSignal   ? 1 : 0; // 삼항 연산자
        int ls2 = cylinder1.lsForwardSignal    ? 1 : 0;  
        int ls3 = cylinder2.lsBackwardSignal   ? 1 : 0; 
        int ls4 = cylinder2.lsForwardSignal    ? 1 : 0;  
        int ls5 = cylinder3.lsBackwardSignal   ? 1 : 0; 
        int ls6 = cylinder3.lsForwardSignal    ? 1 : 0;  
        int ls7 = cylinder4.lsBackwardSignal   ? 1 : 0; 
        int ls8 = cylinder4.lsForwardSignal    ? 1 : 0;
        int s1  = loaderSensor.sensorSignal    ? 1 : 0;
        int s2  = metalSensor.sensorSignal     ? 1 : 0;
        int s3  = proximitySensor.sensorSignal ? 1 : 0;

        string xDataStr = $"{ls1}{ls2}" +
                          $"{ls3}{ls4}" +
                          $"{ls5}{ls6}" +
                          $"{ls7}{ls8}" +
                          $"{s1}" +
                          $"{s2}" +
                          $"{s3}";

        xDataStr = new string(xDataStr.Reverse().ToArray()); // "00101010101"
        int xData = Convert.ToInt32(xDataStr, 2);            // 341

        MxComponent.instance.plcXData[0] = xData;
    }

    private void OnDisconnectBtnClkEvent()
    {
        MxComponent.instance.CloseAsync();
        //int iRet = MxComponent.instance.Close();

        //if (iRet == -1)
        //{
        //    Debug.LogWarning("이미 PLC와 연결 해지된 상태입니다.");
        //}
        //else if (iRet == 0)
        //{
        //    Debug.Log("PLC 연결해지가 완료되었습니다.");
        //}
        //else
        //{
        //    Debug.LogWarning(iRet.ToString("X"));
        //}
    }

    private void OnConnectBtnClkEvent()
    {
        MxComponent.instance.OpenAsync();
        //int iRet = MxComponent.instance.Open();

        //if (iRet == -1)
        //{
        //    Debug.LogWarning("이미 PLC에 연결된 상태입니다.");
        //}
        //else if (iRet == 0)
        //{
        //    Debug.Log("PLC 연결이 완료되었습니다.");
        //}
        //else
        //{
        //    Debug.LogWarning(iRet.ToString("X"));
        //}
    }

    void OnStartBtnClkEvent()
    {
        startSignal = !startSignal;
        Debug.LogWarning("시작버튼이 눌렸습니다.");
    }

    void OnStopBtnClkEvent()
    {
        stopSignal = !stopSignal;
        Debug.LogWarning("스탑버튼이 눌렸습니다.");
    }

    void OnRedLampBtnClkEvent()
    {
        towerLamp.redLamSignal = !towerLamp.redLamSignal;
        Debug.LogWarning("Red Lamp 버튼이 눌렸습니다.");
    }

    void OnYellowLampBtnClkEvent()
    {
        towerLamp.yellowLamSignal = !towerLamp.yellowLamSignal;
        Debug.LogWarning("Yellow Lamp 버튼이 눌렸습니다.");
    }

    void OnGreenLampBtnClkEvent()
    {
        towerLamp.greenLamSignal = !towerLamp.greenLamSignal;
        Debug.LogWarning("Green Lamp 버튼이 눌렸습니다.");
    }

    void OnConvCWBtnClkEvent()
    {
        conveyor.cwSignal = !conveyor.cwSignal;
        Debug.LogWarning("Conv CW 버튼이 눌렸습니다.");
    }

    void OnConvCCWBtnClkEvent()
    {
        conveyor.ccwSignal = !conveyor.ccwSignal;
        Debug.LogWarning("Conv CCW 버튼이 눌렸습니다.");
    }

    public void OnSOL1BtnDownEvent()
    {
        cylinder1.forwardSignal = true;

        Debug.LogWarning("SOL1 ON -> CYL1 Forward...");
    }

    public void OnSOL1BtnUpEvent()
    {
        cylinder1.forwardSignal = false;

        Debug.LogWarning("SOL1 OFF");
    }

    public void OnSOL2BtnDownEvent()
    {
        cylinder1.backwardSignal = true;

        Debug.LogWarning("SOL2 ON -> CYL1 Backward...");
    }

    public void OnSOL2BtnUpEvent()
    {
        cylinder1.backwardSignal = false;

        Debug.LogWarning("SOL2 OFF");
    }

    public void OnSOL3BtnDownEvent()
    {
        cylinder2.forwardSignal = true;

        Debug.LogWarning("SOL3 ON -> CYL2 Forward...");
    }

    public void OnSOL3BtnUpEvent()
    {
        cylinder2.forwardSignal = false;

        Debug.LogWarning("SOL3 OFF -> CYL2 Backward...");
    }

    public void OnSOL4BtnDownEvent()
    {
        cylinder3.forwardSignal = true;

        Debug.LogWarning("SOL4 ON -> CYL3 Forward...");
    }

    public void OnSOL4BtnUpEvent()
    {
        cylinder3.forwardSignal = false;

        Debug.LogWarning("SOL4 OFF -> CYL3 Backward...");
    }

    public void OnSOL5BtnDownEvent()
    {
        cylinder4.forwardSignal = true;

        Debug.LogWarning("SOL5 ON -> CYL4 Forward...");
    }

    public void OnSOL5BtnUpEvent()
    {
        cylinder4.forwardSignal = false;

        Debug.LogWarning("SOL5 OFF -> CYL4 Backward...");
    }

    public void OnLoaderBtnClkEvent()
    {
        StartCoroutine(CoLoadObj());

        Debug.LogWarning("물체가 로드되었습니다.");
    }

    IEnumerator CoLoadObj()
    {
        loader.loadSignal = true;

        yield return new WaitForEndOfFrame(); // 1프레임 대기

        loader.loadSignal = false;
    }
}

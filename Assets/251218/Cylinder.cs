using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// PLC로 부터 신호를 받으면 솔레노이드(단방향 or 양방향)가 작동하고,
/// 공압이 들어와서 실린더의 Rod가 전진 or 후진한다.
/// 속성: 실린더 Rod transform, 실린더 이동을 위한 minPos, maxPos, 속도
///       단방향, 양방향 옵션
/// </summary>
public class Cylinder : MonoBehaviour
{
    enum 솔레노이드타입
    {
        단방향솔레노이드,
        양방향솔레노이드
    }

    [Header("PLC 신호")]
    public bool forwardSignal;
    public bool backwardSignal;

    [Header("실린더 설정")]
    [SerializeField] 솔레노이드타입 type = 솔레노이드타입.단방향솔레노이드;
    [SerializeField] Transform rod;
    [SerializeField] float minPos;
    [SerializeField] float maxPos;
    [SerializeField] float speed = 2;   // 공압밸브 조절시 속도
    [SerializeField] float returnSpeed = 3;   // 단솔일 경우 Rod가 돌아오는 속도
    [SerializeField] bool isMoving = false;     // 현재 실린더가 움직이는지 여부
    [SerializeField] bool isFrontEnd = false;   // 현재 실린더가 앞쪽에 있는지 여부


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(CoMoveForwardBySignal());
        StartCoroutine(CoMoveBackwardBySignal());
    }

    // Update is called once per frame
    void Update()
    {
        // PLC로 부터 계속해서 X00 신호를 받아줌
        if (Input.GetKeyDown(KeyCode.Space))
        {
            forwardSignal = true;
            //MoveCylinderForward();
        }
        else if(Input.GetKeyUp(KeyCode.Space))
        {
            forwardSignal = false;
        }

        // PLC로 부터 계속해서 X01 신호를 받아줌
        if (Input.GetKeyDown(KeyCode.B))
        {
            backwardSignal = true;
            //MoveCylinderBackward();
        }
        else if (Input.GetKeyUp(KeyCode.B))
        {
            backwardSignal = false;
            //MoveCylinderBackward();
        }
    }

    /// <summary>
    /// 반복적으로 PLC의 forwardSignal을 확인하여 현재 멈춰있다면 움직인다.
    /// </summary>
    /// <returns></returns>
    IEnumerator CoMoveForwardBySignal()
    {
        while(true)
        {
            if(type == 솔레노이드타입.단방향솔레노이드)
            {
                // WaitUntil 안의 내용이 참이라면 다음 시퀀스 진행
                yield return new WaitUntil(() => (forwardSignal && !isMoving) || (isFrontEnd && !isMoving));
            }
            else if(type == 솔레노이드타입.양방향솔레노이드)
            {
                // WaitUntil 안의 내용이 참이라면 다음 시퀀스 진행
                yield return new WaitUntil(() => forwardSignal && !isMoving);
            }

            Vector3 front = new Vector3(0, maxPos, 0);

            yield return CoMoveCylinder(front);
        }
    }

    IEnumerator CoMoveBackwardBySignal()
    {
        while (true)
        {
            if(type == 솔레노이드타입.양방향솔레노이드)
            {
                // WaitUntil 안의 내용이 참이라면 다음 시퀀스 진행
                yield return new WaitUntil(() => backwardSignal && !isMoving);

                Vector3 back = new Vector3(0, minPos, 0);

                yield return CoMoveCylinder(back);
            }

            yield return null;  // *주의: 한프레임 대기 없다면 프로그램 정지
        }
    }


    /// <summary>
    /// 실린더 Rod를 앞쪽으로 이동시킨다.
    /// * 주의사항: PLC의 전진신호(X00)가 계속해서 들어올 때는 한번만 함수를 실행해야 함
    /// </summary>
    public void MoveCylinderForward()
    {
        // Rod의 목적지 position
        Vector3 frontPos = new Vector3(0, maxPos, 0);

        // 현재 함수가 실행되면 비동기적으로 반복실행되는 코루틴함수실행
        StartCoroutine(CoMoveCylinder(frontPos));
    }

    public void MoveCylinderBackward()
    {
        Vector3 backPos = new Vector3(0, minPos, 0);

        // 현재 함수가 실행되면 비동기적으로 반복실행되는 코루틴함수실행
        StartCoroutine(CoMoveCylinder(backPos));
    }

    IEnumerator CoMoveCylinder(Vector3 to)
    {
        float curSpeed = speed;

        if(!isMoving)  // false -> true
        {
            isMoving = true;

            while (true)
            {
                if (type == 솔레노이드타입.단방향솔레노이드)
                {
                    // 신호가 끊기는 경우 후진방향으로 방향을 설정
                    if (!forwardSignal)
                    {
                        to = new Vector3(0, minPos, 0);
                        curSpeed = returnSpeed;
                    }
                    else
                        curSpeed = speed;
                }
                else if (type == 솔레노이드타입.양방향솔레노이드)
                {
                    // 양솔일 경우에만 사용: forwardSignal과 backwardSignal이 동시에 들어올 때는 반복문을 멈춰서 실린더를 멈춘다.
                    bool isDualSignal = CheckSignal();
                    if (isDualSignal)
                    {
                        isMoving = false;
                        break;
                    }
                    // 동시신호가 아닌 경우, 방향을 확정!
                    else
                    {
                        if(forwardSignal)
                        {
                            to = new Vector3(0, maxPos, 0);
                        }
                        else if (backwardSignal)
                        {
                            to = new Vector3(0, minPos, 0);
                        }
                    }
                }

                // 실린더 Rod의 이동방향 설정
                Vector3 direction = to - rod.localPosition;

                float distance = direction.magnitude;
                
                if(distance < 0.1f)
                {
                    isMoving = false;

                    if (type == 솔레노이드타입.단방향솔레노이드)
                    {
                        isFrontEnd = true;
                    }

                    break;
                }
                else
                {
                    if (type == 솔레노이드타입.단방향솔레노이드)
                    {
                        isFrontEnd = false;
                    }
                }

                // 크기가 1인 벡터로 만든다.
                direction = Vector3.Normalize(direction);

                // 실린더 Rod를 direction 방향으로 특정 속도로 이동시킨다.
                rod.localPosition = rod.localPosition + (direction * curSpeed * Time.deltaTime);

                yield return null;
            }
        }

        // 양솔일 경우에만 사용: forwardSignal과 backwardSignal이 동시에 들어올 때는 반복문을 멈춰서 실린더를 멈춘다.
        bool CheckSignal()
        {
            if(forwardSignal && backwardSignal)
                return true;
            else
                return false;
        }
    }
}

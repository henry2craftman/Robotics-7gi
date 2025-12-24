using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

/// <summary>
/// 새로운 워커스레드(sub thread)를 만들어서 메인스레이드와 작업을 함께 한다.
/// - 작업: data 변수의 값을 함께 공유하며 연산을 한다.
/// (메인스레드: 더하기 연산, 워커스레드: 빼기연산)
/// 속성: data 변수, 상호배제(Mutex)용 변수
/// </summary>
public class TaskManager : MonoBehaviour
{
    public int data;
    public object lockObj = new object(); // 상호배제(Mutex)용 변수
    public int interval = 30;
    CancellationTokenSource tokenSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tokenSource = new CancellationTokenSource(); // Task를 관리하는 관리자 역할
        var token = tokenSource.Token;               // Task에 정보를 저장하는 역할
        Task thread1 = Task.Run(Minus, tokenSource.Token);  // 스레드 생성 = 일 할당 + token등록
    }

    /// <summary>
    /// Unity LifeCycle 함수에서 객체가 해지될때 실행되는 이벤트 함수
    /// </summary>
    private void OnDestroy()
    {
        StopThread(tokenSource);
    }

    void StopThread(CancellationTokenSource ts)
    {
        ts.Cancel();  // 쓰레드 종료
        ts.Dispose(); // 쓰레드 종료 토큰소스를 메모리에서 해지
    }

    // Update is called once per frame
    void Update()
    {
        lock (lockObj) // 3. Mutex(상호배제): 여러 스레드가 공유자원에 접근을 순서대로 하도록함
        {
            data++; // 1. 메인 스레드가 공유자원을 더한다.
        }

        Debug.Log("Update: " + data);
    }

    /// <summary>
    /// 워커스레드가 실행할 반복 함수(메인스레드의 update와 동일한 용도)
    /// </summary>
    async void Minus()
    {
        while(true)
        {
            lock(lockObj) // 3. Mutex(상호배제): 여러 스레드가 공유자원에 접근을 순서대로 하도록함
            {
                data--; // 2. 워커 스레드가 공유자원을 뺀다.
            }

            Debug.Log("Worker Thread: " + data);

            await Task.Delay(interval, tokenSource.Token); // 스레드 반복의 지연
        }
    }
}

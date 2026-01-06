#define MASTER // MASTER / SLAVE

using Firebase;
using Firebase.Database;
using System.Collections;
using UnityEngine;

/// <summary>
/// Firebase Realtime DB에 PLC 정보를 Update
/// 속성: PLC 정보를 저장하기위한 문자열변수 input, output, dbURL, dbRef
/// </summary>
public class DBManager : MonoBehaviour
{
    [SerializeField] string dbURL;
    DatabaseReference dbRef;
    [SerializeField] float interval = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Firebase 콘솔 객체와 연동
        FirebaseApp.DefaultInstance.Options.DatabaseUrl = new System.Uri(dbURL);

        // Database Reference 참조하기
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void OnConnectBtnClkEvent()
    {
        StartCoroutine(CoUpdateData());
    }

    IEnumerator CoUpdateData()
    {
#if MASTER
        yield return new WaitUntil(() => MxComponent.instance.isConnected == true);

        while (MxComponent.instance.isConnected)
        {
            dbRef.Child("isConnected").SetValueAsync(MxComponent.instance.isConnected);

            dbRef.Child("data").Child("input").SetValueAsync(MxComponent.instance.input);
            dbRef.Child("data").Child("output").SetValueAsync(MxComponent.instance.output);

            yield return new WaitForSeconds(interval);
        }
#elif SLAVE

        dbRef.Child("isConnected").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot d = task.Result;
                string isConnectedStr = d.GetRawJsonValue();

                bool isConverted = bool.TryParse(isConnectedStr, out MxComponent.instance.isConnected);
                if (!isConverted)
                    Debug.LogWarning("데이터 변환이 실패하였습니다. str -> bool");
            }
        });

        yield return new WaitUntil(() => MxComponent.instance.isConnected == true);

        while(true)
        {
            dbRef.Child("data").Child("output").GetValueAsync().ContinueWith(task =>
            {
                DataSnapshot data = task.Result;

                string json = data.GetRawJsonValue();

                json = json.Replace("\"", "");

                MxComponent.instance.output = json; // "565,56,32" -> 565,56,32;
            });

            yield return new WaitForSeconds(interval);
        }
#endif
    }
}

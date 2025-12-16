using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// UI들을 참조하고, 버튼을 누르면 ID, PW가 일치하는지 확인 후, 
/// 일치하면 로그인이 된다.
/// 그렇지 않으면 ID 또는 PW가 일치하지 않습니다. 출력
/// </summary>
public class UIManager : MonoBehaviour
{
    [SerializeField] TMP_InputField idInput;
    [SerializeField] TMP_InputField pwInput;
    [SerializeField] string id = "abc@gmail.com";
    [SerializeField] string pw = "12345678";
    bool isIDCorrect = false;
    bool isPWCorrect = false;

    // OK버튼과 연결할 이벤트 메서드
    public void OnOKBtnClkEvent()
    {
        if(idInput.text == id)
        {
            isIDCorrect = true;
        }

        if(pwInput.text == pw)
        {
            isPWCorrect = true;
        }

        if(isIDCorrect && isPWCorrect)
        {
            print("로그인이 완료되었습니다.");

            isIDCorrect = isPWCorrect = false;

            SceneManager.LoadScene("Progress"); // 바로 실행
            // Scene 용량이 너무 커서 로딩이 오래걸리면 강제종료할 경우
            // --> Progress Bar or Progress Circle
        }
        else
        {
            print("ID 또는 PW가 일치하지 않습니다. 다시 확인후 시도해 주세요.");

            isIDCorrect = isPWCorrect = false;
        }
    }
}

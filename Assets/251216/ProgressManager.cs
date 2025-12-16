using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 무거씬이 완전히 로드될 때 까지 작동할 Progress Bar
/// 속성: image, text
/// </summary>
public class ProgressManager : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] Image image;

    void Start()
    {
        StartCoroutine(CoStartProgressBar());
    }

    IEnumerator CoStartProgressBar()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("CubeMove");
        operation.allowSceneActivation = false;     // 버튼을 누르기 전에는 활성화 되지 않도록.

        while (!operation.isDone)
        {
            float progress = operation.progress;
            print(progress * 100 + "%");
            text.text = progress * 100 + "%";
            image.fillAmount = progress;

            if (progress >= 0.9f)
            {
                print("계속 하시려면 space 키를 눌러주세요.");
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    operation.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }
}


using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1버튼을 누르면 BGM 재생, 2버튼을 누르면 punch1 재생
/// 속성: AudioSource, audioclip들
/// </summary>
public class AudioManager : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField] List<AudioClip> clips = new List<AudioClip>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // BGM 재생
            audioSource.clip = clips[0];
            audioSource.Play();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // punch1 재생
            audioSource.clip = clips[1];
            audioSource.Play();
        }
    }
}

using UnityEngine;

public class PaperAudio : MonoBehaviour
{
    private bool isOpen = false;

    void OnEnable()
    {
        AudioManager.Instance.PlayDiaryOpen();
        isOpen = true;
    }

    void OnDisable()
    {
        if (isOpen) 
        {
            AudioManager.Instance.PlayDiaryClose();
            isOpen = false;
        }
    }
}

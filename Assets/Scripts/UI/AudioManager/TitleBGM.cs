using UnityEngine;

public class TitleBGM : MonoBehaviour
{
    void Start()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySceneBGM("TitleScene");
    }
}

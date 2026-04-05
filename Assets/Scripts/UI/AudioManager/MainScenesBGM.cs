using UnityEngine;

public class MainScenesBGM : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.Instance.PlaySceneBGM("MapScene");
    }
}

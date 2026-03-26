using UnityEngine;
using UnityEngine.UI;

public class GameQuitUI : MonoBehaviour
{
    [SerializeField] private Button _QuitYesButton;
    [SerializeField] private Button _QuitNoButton;

    public void OnClickYesButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    public void OnClickNoButton()
    {
        gameObject.SetActive(false);
    }
}

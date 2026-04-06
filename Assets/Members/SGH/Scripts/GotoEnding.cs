using UnityEngine;
using UnityEngine.SceneManagement;

public class GotoEnding : MonoBehaviour
{
    public void OnClickGotoEnd()
    {
        SceneManager.LoadScene(2);
        AudioManager.Instance.PlaySceneBGM("TitleScene");
    }

}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartUI : MonoBehaviour
{
   [SerializeField] private Button _startYesButton;
   [SerializeField] private Button _startNoButton;
    
    public void OnClickYesButton()
    {
        SceneManager.LoadScene(1);
        AudioManager.Instance.PlaySceneBGM("MapScene");
    }
    
    public void OnClickNoButton()
    {
        gameObject.SetActive(false);
    }
}

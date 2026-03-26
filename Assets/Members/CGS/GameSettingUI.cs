using UnityEngine;
using UnityEngine.UI;

public class GameSettingUI : MonoBehaviour
{
    [SerializeField] private Button _backButton;

    public void OnClickBackButton()
    {
        gameObject.SetActive(false);
    }
}

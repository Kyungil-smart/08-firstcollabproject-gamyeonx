using UnityEngine;
using UnityEngine.UI;

public class GameSettingUI : MonoBehaviour
{
    [SerializeField] private Button _backButton;

    [Header("셋팅창에서 카메라 멈추기 위한 참조")]
    [SerializeField] private CameraController cameraController;

    public void OnClickBackButton()
    {
        gameObject.SetActive(false);
        if (cameraController != null) cameraController.canMove = true;
        Time.timeScale = 1f;
    }
}

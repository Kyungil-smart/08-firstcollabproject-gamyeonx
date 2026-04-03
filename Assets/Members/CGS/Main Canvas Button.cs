using UnityEngine;
using UnityEngine.UI;

public class MainCanvasButton : MonoBehaviour
{
    [SerializeField] private Button NewGame;
    [SerializeField] private Button LoadGame;
    [SerializeField] private Button Quit;
    [SerializeField] private Button Setting;

    [SerializeField] private GameObject _newGameCanvas;
    [SerializeField] private GameObject _loadGameCanvas;
    [SerializeField] private GameObject _settingCanvas;
    [SerializeField] private GameObject _quitCanvas;

    [Header("셋팅창에서 카메라 멈추기 위한 참조")]
    [SerializeField] private CameraController cameraController;

    public void OnClickNewGame()
    {
        _newGameCanvas.SetActive(true);
    }

    public void OnClickLoadGame()
    {
        _loadGameCanvas.SetActive(true);
    }

    public void OnClickSettingCanvas()
    {
        _settingCanvas.SetActive(true);
        if (cameraController != null) cameraController.canMove = false;
        Time.timeScale = 0f;
    }

    public void OnClickSettingQuit()
    {
        _quitCanvas.SetActive(true);
    }
}

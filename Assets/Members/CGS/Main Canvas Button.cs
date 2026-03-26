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
    }

    public void OnClickSettingQuit()
    {
        _quitCanvas.SetActive(true);
    }
}

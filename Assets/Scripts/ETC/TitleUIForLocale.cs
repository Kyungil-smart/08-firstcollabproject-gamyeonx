using UnityEngine;

public class TitleUIForLocale : MonoBehaviour
{
    [SerializeField] GameObject uiRoot; 

    void Awake()
    {
        uiRoot.SetActive(false); 
        LocaleManager.OnLocaleReady += Show;
    }

    void Show()
    {
        uiRoot.SetActive(true);
        LocaleManager.OnLocaleReady -= Show;
    }

    void OnDestroy()
    {
        LocaleManager.OnLocaleReady -= Show;
    }
}

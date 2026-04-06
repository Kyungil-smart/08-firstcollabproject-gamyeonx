using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocaleManager : MonoBehaviour
{
    public static event System.Action OnLocaleReady;

    bool isChanging;
    const string LOCALE_KEY = "LOCALE_INDEX";

    void Start()
    {
        LoadLocale();
    }

    public void ChangeLocale(int index)
    {
        if (isChanging) return;

        PlayerPrefs.SetInt(LOCALE_KEY, index);
        PlayerPrefs.Save();

        StartCoroutine(ChangeRoutine(index));
    }

    void LoadLocale()
    {
        int index = PlayerPrefs.GetInt(LOCALE_KEY, 0);
        StartCoroutine(ChangeRoutine(index));
    }

    IEnumerator ChangeRoutine(int index)
    {
        isChanging = true;
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale =
            LocalizationSettings.AvailableLocales.Locales[index];
        isChanging = false;

        OnLocaleReady?.Invoke();
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsPanel : MonoBehaviour
{


    [Header("볼륨")]

    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        if (bgmSlider != null)
        {
            bgmSlider.value = AudioManager.Instance.BGMVolume;
            bgmSlider.onValueChanged.AddListener(AudioManager.Instance.SetBGMVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = AudioManager.Instance.SFXVolume;
            sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
        }

        if (masterSlider != null)
        {
            masterSlider.value = AudioManager.Instance.MasterVolume;
            masterSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
        }
    }
    public void ResetAudio()
    {
        AudioManager.Instance.ResetVolume();

        if (masterSlider != null) masterSlider.value = AudioManager.Instance.MasterVolume;
        if (bgmSlider != null) bgmSlider.value = AudioManager.Instance.BGMVolume;
        if (sfxSlider != null) sfxSlider.value = AudioManager.Instance.SFXVolume;
    }
}


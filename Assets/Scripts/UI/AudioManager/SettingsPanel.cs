using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    [Header("볼륨 슬라이더 (0~100)")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("볼륨 텍스트")]
    [SerializeField] private TextMeshProUGUI masterText;
    [SerializeField] private TextMeshProUGUI bgmText;
    [SerializeField] private TextMeshProUGUI sfxText;

    private void Start()
    {
        // 슬라이더 최대/최소 값 설정
        if (masterSlider != null) { masterSlider.minValue = 0f; masterSlider.maxValue = 100; }
        if (bgmSlider != null) { bgmSlider.minValue = 0f; bgmSlider.maxValue = 100; }
        if (sfxSlider != null) { sfxSlider.minValue = 0f; sfxSlider.maxValue = 100; }

        // 초기화 및 이벤트 등록
        InitializeSlider(masterSlider, AudioManager.Instance.MasterVolume * 100, masterText, AudioManager.Instance.SetMasterVolume);
        InitializeSlider(bgmSlider, AudioManager.Instance.BGMVolume * 100, bgmText, AudioManager.Instance.SetBGMVolume);
        InitializeSlider(sfxSlider, AudioManager.Instance.SFXVolume * 100, sfxText, AudioManager.Instance.SetSFXVolume);
    }

    private void InitializeSlider(Slider slider, float startValue, TextMeshProUGUI text, System.Action<float> setVolume)
    {
        if (slider == null) return;

        slider.onValueChanged.RemoveAllListeners();

        slider.SetValueWithoutNotify(startValue);
        UpdateText(text, startValue);

        slider.onValueChanged.AddListener(v =>
        {
            setVolume(v / 100f);  // AudioManager는 0~1 값 사용
            UpdateText(text, v);   // 텍스트는 0~100 표시
        });
    }

    private void UpdateText(TextMeshProUGUI text, float value)
    {
        if (text != null)
            text.text = Mathf.RoundToInt(value).ToString();
    }

    public void ResetAudio()
    {
        AudioManager.Instance.ResetVolume();

        if (masterSlider != null)
        {
            float v = AudioManager.Instance.MasterVolume * 100;
            masterSlider.SetValueWithoutNotify(v);
            UpdateText(masterText, v);
        }

        if (bgmSlider != null)
        {
            float v = AudioManager.Instance.BGMVolume * 100;
            bgmSlider.SetValueWithoutNotify(v);
            UpdateText(bgmText, v);
        }

        if (sfxSlider != null)
        {
            float v = AudioManager.Instance.SFXVolume * 100;
            sfxSlider.SetValueWithoutNotify(v);
            UpdateText(sfxText, v);
        }
    }
}
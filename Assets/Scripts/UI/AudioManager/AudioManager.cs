using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip titleBGM;
    [SerializeField] private AudioClip mapBGM;
    [SerializeField] private AudioClip battleBGM;
    [SerializeField] private AudioClip shopBGM;
    [SerializeField] private AudioClip eventBGM;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip buttonHoverSFX;
    [SerializeField] private AudioClip buttonPressSFX;
    [SerializeField] private AudioClip mouseClickSFX;
    [SerializeField] private AudioClip victorySFX;
    [SerializeField] private AudioClip defeatSFX;

    private float _masterVolume = 1f;
    private float _bgmVolume = 0.1f;
    private float _sfxVolume = 0.3f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        _bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.1f);
        _sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.1f);
        bgmSource.volume = _bgmVolume;
        sfxSource.volume = _sfxVolume;

        ApplyVolume();
    }

    private void ApplyVolume()
    {
        bgmSource.volume = _bgmVolume * _masterVolume;
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource.clip == clip) return;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, _sfxVolume * _masterVolume);
    }

    public void PlaySceneBGM(string sceneName)
    {
        AudioClip clip = sceneName switch
        {
            "TitleScene" => titleBGM,
            "MapScene" => mapBGM,
            "BattleScene" => battleBGM,
            "ShopScene" => shopBGM,
            "EventScene" => eventBGM,
            _ => null
        };
        if (clip != null) PlayBGM(clip);
    }

    public void SetMasterVolume(float volume)
    {
        _masterVolume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
        ApplyVolume();
    }

    public void SetBGMVolume(float volume)
    {
        _bgmVolume = volume;
        bgmSource.volume = volume;
        PlayerPrefs.SetFloat("BGMVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    public void ResetVolume()
    {
        _masterVolume = 1f;
        _bgmVolume = 0.1f;
        _sfxVolume = 0.3f;

        PlayerPrefs.SetFloat("MasterVolume", _masterVolume);
        PlayerPrefs.SetFloat("BGMVolume", _bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", _sfxVolume);
        PlayerPrefs.Save();

        ApplyVolume();
    }

    public float MasterVolume => _masterVolume;
    public float BGMVolume => _bgmVolume;
    public float SFXVolume => _sfxVolume;

    public void PlayButtonHoverSFX() => PlaySFX(buttonHoverSFX);
    public void PlayButtonPressSFX() => PlaySFX(buttonPressSFX);
    public void PlayMouseClickSFX() => PlaySFX(mouseClickSFX);
    public void PlayVictorySFX() => PlaySFX(victorySFX);
    public void PlayDefeatSFX() => PlaySFX(defeatSFX);
}
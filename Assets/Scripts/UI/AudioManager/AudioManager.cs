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
    [SerializeField] private AudioClip buildSFX;
    [SerializeField] private AudioClip deleteSFX;
    [SerializeField] private AudioClip upgradeSFX;
    [SerializeField] private AudioClip enterSFX;


    private const float DefaultMaster = 1f;
    private const float DefaultBGM = 0.1f;
    private const float DefaultSFX = 0.3f;

    private float _masterVolume;
    private float _bgmVolume;
    private float _sfxVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _masterVolume = PlayerPrefs.GetFloat("MasterVolume", DefaultMaster);
        _bgmVolume = PlayerPrefs.GetFloat("BGMVolume", DefaultBGM);
        _sfxVolume = PlayerPrefs.GetFloat("SFXVolume", DefaultSFX);

        ApplyVolume();
    }

    private void ApplyVolume()
    {
        if (bgmSource != null) bgmSource.volume = _bgmVolume * _masterVolume;
        if (sfxSource != null) sfxSource.volume = _sfxVolume * _masterVolume;
    }

    public void ResetVolume()
    {
        _masterVolume = DefaultMaster;
        _bgmVolume = DefaultBGM;
        _sfxVolume = DefaultSFX;

        PlayerPrefs.SetFloat("MasterVolume", _masterVolume);
        PlayerPrefs.SetFloat("BGMVolume", _bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", _sfxVolume);
        PlayerPrefs.Save();

        ApplyVolume();
    }

    public void SetMasterVolume(float volume) { _masterVolume = volume; PlayerPrefs.SetFloat("MasterVolume", volume); PlayerPrefs.Save(); ApplyVolume(); }
    public void SetBGMVolume(float volume) { _bgmVolume = volume; PlayerPrefs.SetFloat("BGMVolume", volume); PlayerPrefs.Save(); ApplyVolume(); }
    public void SetSFXVolume(float volume) { _sfxVolume = volume; PlayerPrefs.SetFloat("SFXVolume", volume); PlayerPrefs.Save(); ApplyVolume(); }

    public float MasterVolume => _masterVolume;
    public float BGMVolume => _bgmVolume;
    public float SFXVolume => _sfxVolume;

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

    public void PlayButtonHoverSFX() => PlaySFX(buttonHoverSFX);
    public void PlayButtonPressSFX() => PlaySFX(buttonPressSFX);
    public void PlayMouseClickSFX() => PlaySFX(mouseClickSFX);
    public void PlayVictorySFX() => PlaySFX(victorySFX);
    public void PlayDefeatSFX() => PlaySFX(defeatSFX);
    public void PlayBuildButtonSFX() => PlaySFX(buildSFX); 
    public void PlayDeleteButtonSFX() => PlaySFX(deleteSFX); 
    public void PlayUpgradeButtonSFX() => PlaySFX(upgradeSFX); 
    public void PlayEnterButtonSFX() => PlaySFX(enterSFX);
}
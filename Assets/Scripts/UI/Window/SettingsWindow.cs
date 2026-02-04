using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsWindow : MonoBehaviour
{
    [SerializeField] DataSaver dataSaver;
    [SerializeField] UserGameSettings settings;
    [SerializeField] SettingsManager settingsManager;

    [SerializeField] Slider musicSlider, soundSlider;
    [SerializeField] Toggle fullscreenToggle;

    [SerializeField] AudioMixer musicMixer, soundMixer;

    public void ValidateSettings()
    {
        musicSlider.value = settings.MusicVolume;
        soundSlider.value = settings.SoundVolume;
        fullscreenToggle.isOn = settings.IsFullscreen;
    }
    private void OnEnable()
    {
        ValidateSettings();

        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        soundSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
    }
    private void OnDisable()
    {
        musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        soundSlider.onValueChanged.RemoveListener(OnSoundVolumeChanged);
        fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);

        dataSaver.Save();
    }
     
    public void OnMusicVolumeChanged(float value)
    {
        settings.MusicVolume = value;
        settingsManager.RefreshMusicVolume();
    }
    public void OnSoundVolumeChanged(float value)
    {
        settings.SoundVolume = value;
        settingsManager.RefreshSoundVolume();
    }
    public void OnFullscreenChanged(bool value)
    {
        settings.IsFullscreen = value;
        settingsManager.RefreshFullscreen();
    }
}

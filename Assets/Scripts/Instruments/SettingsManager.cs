using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] UserGameSettings settings;

    [SerializeField] List<AudioSource> sounds;
    [SerializeField] AudioSource music;

    public void Init()
    {
        sounds = Resources.FindObjectsOfTypeAll<AudioSource>().Where(x => x != music).ToList();
        RefreshFullscreen();
        RefreshMusicVolume();
        RefreshSoundVolume();
    }

    public void RefreshSoundVolume()
    {
        float volume = settings.SoundVolume;

        foreach (var sound in sounds)
        {
            if (sound)
                sound.volume = volume;
        }
    }
    public void RefreshMusicVolume()
    {
        music.volume = settings.MusicVolume;
    }
    public void RefreshFullscreen()
    {
        Screen.fullScreen = settings.IsFullscreen;
    }

    private void Start() => Init();
}

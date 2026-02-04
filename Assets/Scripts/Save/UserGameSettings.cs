using UnityEngine;

[CreateAssetMenu(fileName = "userPlayerSettings", menuName = "SO/User Player Settings")]
public class UserGameSettings : ScriptableObject
{
    public float MusicVolume;
    public float SoundVolume;
    public bool IsFullscreen;
}

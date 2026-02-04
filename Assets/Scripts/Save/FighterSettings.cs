using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "fighterSettings", menuName = "SO/Fighter Settings")]
public class FighterSettings : ScriptableObject
{
    public string Name;
    [DoNotSerialize] public GameObject Model;
    [DoNotSerialize] public Sprite Icon;
    [DoNotSerialize] public AudioClip HurtSound1, HurtSound2;

    public float Health;
    public float ArmKickDamage;
    public float LegKickDamage;
    public float ArmKickSpeed;
    public float LegKickSpeed;
    public float StunProbability;
    public Vector2 StunTimeRange;
}

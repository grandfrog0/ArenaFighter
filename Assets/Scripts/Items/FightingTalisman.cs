using UnityEngine;

[CreateAssetMenu(fileName = "talisman", menuName = "SO/Fighting Talisman")]
public class FightingTalisman : FightingItem
{
    public string Name;
    public Sprite Icon;

    public Sprite EffectSprite;
    public float UseTime;
    public float RechargeTime;
}

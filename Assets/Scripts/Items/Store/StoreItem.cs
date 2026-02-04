using UnityEngine;

[CreateAssetMenu(fileName = "shopItem", menuName = "SO/Shop Item")]
public class StoreItem : ScriptableObject
{
    public string Item;
    public ItemType Type;
    public Sprite Icon;

    public ValuteType Valute;
    public int Cost;
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "storeConfig", menuName = "SO/Store Config")]
public class StoreConfig : ScriptableObject
{
    public List<StoreItem> Items;       // store catalog
    public List<StoreItem> BoughtItems; // user items
}

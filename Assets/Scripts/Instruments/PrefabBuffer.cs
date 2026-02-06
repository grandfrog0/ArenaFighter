using System.Collections.Generic;
using UnityEngine;

public class PrefabBuffer : MonoBehaviour
{
    private static PrefabBuffer _instance;

    public static FighterSettings GetFighter(int id)
        => id != -1 ? _instance.items.Fighters[id] : null;
    public static FightingTalisman GetTalisman(int id)
        => id != -1 ? _instance.items.Talismans[id] : null;
    public static FightingElixir GetElixir(int id)
        => id != -1 ? _instance.items.Elixirs[id] : null;

    [SerializeField] GameItems items;
    private void Awake() => _instance = this;
}

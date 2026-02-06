using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

[CreateAssetMenu(fileName = "gameItems", menuName = "SO/Game Items")]
public class GameItems : ScriptableObject
{
    public List<FighterSettings> Fighters;
    public List<FightingTalisman> Talismans;
    public List<FightingElixir> Elixirs;
}

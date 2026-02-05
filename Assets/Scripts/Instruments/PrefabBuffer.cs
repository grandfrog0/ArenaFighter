using System.Collections.Generic;
using UnityEngine;

public class PrefabBuffer : MonoBehaviour
{
    public static PrefabBuffer Instance { get; private set; }

    public List<FighterSettings> fighters; 
    public List<FightingTalisman> fighters; 
    public List<FighterSettings> fighters;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }
}

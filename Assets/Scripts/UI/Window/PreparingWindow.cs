using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PreparingWindow : MonoBehaviour
{
    [SerializeField] StoreConfig store;
    [SerializeField] RoundManager roundManager;

    [SerializeField] List<FighterSettings> fighters;
    [SerializeField] List<FightingTalisman> talismans;
    [SerializeField] List<StoreItem> elixirs;

    [SerializeField] List<Toggle> fighterButtons;
    [SerializeField] List<Toggle> talismanButtons;
    [SerializeField] List<Toggle> elixirButtons;

    private FighterSettings _selectedFighter;
    private FightingTalisman _selectedTalisman;
    private StoreItem _selectedElixir;

    public void Continue()
    {
        roundManager.InitPlayerServerRpc(fighters.IndexOf(_selectedFighter)/*, _selectedTalisman, _selectedElixir*/);
        roundManager.StartGame();
    }

    private void OnEnable()
    {
        for (int i = 0; i < fighterButtons.Count; i++)
        {
            int a = i;
            if (IsFighterAvailable(i))
            {
                fighterButtons[i].onValueChanged.AddListener(x => SelectFighter(a));
                fighterButtons[i].gameObject.SetActive(true);
            }
            else
            {
                fighterButtons[i].gameObject.SetActive(false);
            }
        }
        _selectedFighter = fighters[0];

        for (int i = 0; i < talismanButtons.Count; i++)
        {
            int a = i;
            if (IsTalismanAvailable(i))
            { 
                talismanButtons[i].onValueChanged.AddListener(x => SelectTalisman(a));
                talismanButtons[i].gameObject.SetActive(true);
            }
            else 
            { 
                talismanButtons[i].gameObject.SetActive(false);
            }
        }
        _selectedTalisman = talismans.Find(x => store.BoughtItems.Any(y => y.Item == x.Name));

        for (int i = 0; i < elixirButtons.Count; i++)
        {
            int a = i;
            if (IsElixirAvailable(i)) 
            { 
                elixirButtons[i].onValueChanged.AddListener(x => SelectElixir(a));
                elixirButtons[i].gameObject.SetActive(true);
            }
            else 
            { 
                elixirButtons[i].gameObject.SetActive(false);
            }
        }
    }
    private void OnDisable()
    {
        for (int i = 0; i < fighterButtons.Count; i++)
        {
            fighterButtons[i].onValueChanged.RemoveAllListeners();
        }

        for (int i = 0; i < talismanButtons.Count; i++)
        {
            talismanButtons[i].onValueChanged.RemoveAllListeners();
        }

        for (int i = 0; i < elixirButtons.Count; i++)
        {
            elixirButtons[i].onValueChanged.RemoveAllListeners();
        }
    }

    private void SelectFighter(int index)
    {
        _selectedFighter = fighters[index];
    }
    private void SelectTalisman(int index)
    {
        _selectedTalisman = talismans[index];
    }
    private void SelectElixir(int index)
    {
        _selectedElixir = elixirs[index];
    }

    private bool IsFighterAvailable(int index)
        => store.BoughtItems.Any(x => x.Item == fighters[index].Name);
    private bool IsTalismanAvailable(int index)
        => store.BoughtItems.Any(x => x.Item == talismans[index].Name);
    private bool IsElixirAvailable(int index)
        => store.BoughtItems.Contains(elixirs[index]);
}

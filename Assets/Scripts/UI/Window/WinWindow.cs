using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinWindow : MonoBehaviour
{
    [SerializeField] DataSaver dataSaver;
    [SerializeField] StoreConfig store;
    [SerializeField] UserData userData;

    [SerializeField] FormattedText statisticsText;
    [SerializeField] FormattedText diamondsText, coinsText;
    [SerializeField] Image newItemImage;

    [SerializeField] Vector2Int coinsRange = new(50, 100);
    [SerializeField] Vector2Int diamondsRange = new(3, 5);
    [SerializeField] float talismanDropProbability = 30;
    [SerializeField] float elixirDropProbability = 70;

    [SerializeField] AudioSource au;

    public void Win()
    {
        GetPrize(out float coins, out float diamonds, out StoreItem item);
        ValidateView(coins, diamonds, item);
        dataSaver.Save();

        au.Play();
    }

    private void ValidateView(float coins, float diamonds, StoreItem item)
    {
        statisticsText.SetValue(userData.Wins, userData.Loses);
        diamondsText.SetValue(diamonds);
        coinsText.SetValue(coins);

        if (item != null)
        {
            newItemImage.enabled = true;
            newItemImage.sprite = item.Icon;
        }
        else
        {
            newItemImage.enabled = false;
        }
    }

    private void GetPrize(out float coins, out float diamonds, out StoreItem newItem)
    {
        userData.Wins++;

        coins = Random.Range(coinsRange.x, coinsRange.y);
        ValuteManager.Instance.AddValute(ValuteType.Coins, coins);

        diamonds = Random.Range(diamondsRange.x, diamondsRange.y);
        ValuteManager.Instance.AddValute(ValuteType.Diamonds, diamonds);

        newItem = null;

        if (Random.Range(0, 100) < elixirDropProbability)
        {
            List<StoreItem> elixirs = GetAvailableItems(ItemType.Elixir);
            if (elixirs.Count != 0)
            {
                int index = Random.Range(0, elixirs.Count);
                newItem = elixirs[index];
            }
        }
        if (newItem == null && Random.Range(0, 100) < talismanDropProbability)
        {
            List<StoreItem> talismans = GetAvailableItems(ItemType.Talisman);
            if (talismans.Count != 0)
            {
                int index = Random.Range(0, talismans.Count);
                newItem = talismans[index];
            }
        }

        if (newItem != null)
        {
            store.BoughtItems.Add(newItem);
        }
    }

    private List<StoreItem> GetAvailableItems(ItemType type)
        => store.Items.Where(x => x.Type == type && !store.BoughtItems.Contains(x)).ToList();
}

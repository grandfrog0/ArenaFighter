using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StoreWindow : MonoBehaviour
{
    [SerializeField] DataSaver dataSaver;
    [SerializeField] StoreConfig storeConfig;

    [SerializeField] AudioSource buySound;

    [SerializeField] List<ItemViewer> items;

    public void OnItemClick(int index)
    {
        StoreItem item = storeConfig.Items[index];

        if (!IsItemBought(index))
        {
            if (ValuteManager.Instance.GetValuteCount(item.Valute) < item.Cost)
                return;

            BuyItem(item);
            items[index].gameObject.SetActive(false);
            buySound.Play();
            Debug.Log(item.Item + " bought@");
        }
    }

    public void BuyItem(StoreItem item)
    {
        ValuteManager.Instance.AddValute(item.Valute, -item.Cost);
        storeConfig.BoughtItems.Add(item);
    }

    private void OnEnable()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (IsItemBought(i))
            {
                items[i].gameObject.SetActive(false);
                continue;
            }
            items[i].gameObject.SetActive(true);

            int a = i;
            items[i].button.onClick.AddListener(() => OnItemClick(a));
        }
    }
    private void OnDisable()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].button.onClick.RemoveAllListeners();
        }

        dataSaver.Save();
    }

    private bool IsItemBought(int index)
        => storeConfig.BoughtItems.Contains(storeConfig.Items[index]);
}

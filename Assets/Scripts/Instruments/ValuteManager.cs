using UnityEngine;
using UnityEngine.Events;

public class ValuteManager : MonoBehaviour
{
    public static ValuteManager Instance;

    public UnityEvent<float> OnCoinsCountChanged = new();
    public UnityEvent<float> OnDiamondsCountChanged = new();

    [SerializeField] UserData userData;

    public void Refresh()
    {
        OnCoinsCountChanged.Invoke(userData.Coins);
        OnDiamondsCountChanged.Invoke(userData.Diamonds);
    }

    private void Start()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(gameObject);

        OnCoinsCountChanged.Invoke(userData.Coins);
        OnDiamondsCountChanged.Invoke(userData.Diamonds);
    }

    public float GetValuteCount(ValuteType valute)
        => valute switch
        {
            ValuteType.Coins => userData.Coins,
            ValuteType.Diamonds => userData.Diamonds,
            _ => 0
        };

    public void AddValute(ValuteType valute, float count)
    {
        switch (valute)
        {
            case ValuteType.Coins:
                userData.Coins += count;
                OnCoinsCountChanged.Invoke(userData.Coins);
                break;

            case ValuteType.Diamonds:
                userData.Diamonds += count;
                OnDiamondsCountChanged.Invoke(userData.Diamonds);
                break;
        }
    }

    public void AddValuteListener(ValuteType valute, UnityAction<float> action)
    {
        switch (valute)
        {
            case ValuteType.Coins:
                OnCoinsCountChanged.AddListener(action);
                break;

            case ValuteType.Diamonds:
                OnDiamondsCountChanged.AddListener(action);
                break;
        }
    }

    public void RemoveValuteListener(ValuteType valute, UnityAction<float> action)
    {
        switch (valute)
        {
            case ValuteType.Coins:
                OnCoinsCountChanged.RemoveListener(action);
                break;

            case ValuteType.Diamonds:
                OnDiamondsCountChanged.RemoveListener(action);
                break;
        }
    }
}

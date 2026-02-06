using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class SearchOpponentWindow : MonoBehaviour
{
    [SerializeField] UnityEvent OnConnected = new();
    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }
    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong id)
    {
        if (NetworkManager.Singleton.ConnectedClients.Count == 2)
            OnConnected.Invoke();
    }
}

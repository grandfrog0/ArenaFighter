using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    public UnityEvent OnConnected = new();
    public UnityEvent OnDisconnected = new();
    [SerializeField] GameObject playerPrefab;
    private List<ulong> _waitingClients = new();
    private List<GameObject> _spawnedPlayers = new();
    //[SerializeField] List<PlayerConfig> playerConfigs = new();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            _waitingClients.Clear();
            _spawnedPlayers.Clear();
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        _waitingClients.Add(clientId);
        if (_waitingClients.Count == 2)
        {
            StartGameClientRpc();
        }
    }

    public void SetPlayerConfig(PlayerInfo info, ulong id)
    {
        LogIsServerClientRpc(id);

        Vector3 pos = new Vector3(Random.Range(-2, 2), 2, Random.Range(-2, 2));
        GameObject player = Instantiate(playerPrefab, pos, Quaternion.identity);
        PlayerController controller = player.GetComponent<PlayerController>();
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id);

        //PlayerConfig config = playerConfigs[info.SelectedPlayerIndex];
        //NetworkObject model = Instantiate(config.Model, pos, player.transform.rotation).GetComponent<NetworkObject>();
        //model.transform.Translate(0, -1, 0);

        //model.Spawn();

        //Debug.Log(model.TrySetParent(controller.ModelParent));

        //controller.InitClientRpc();
        //controller.GetComponent<Entity>().InitClientRpc(config.Health, config.Damage1, config.Damage2, config.Cooldown1, config.Cooldown2, config.StunningProbability, config.StunningTimeRange);
        _spawnedPlayers.Add(player);
    }

    [ClientRpc]
    void LogIsServerClientRpc(ulong id)
    {
        //Debugger.Log(id, IsServer);
    }

    public void StopGame()
    {
        //Debugger.Log(IsOwner, IsServer);
        OnDisconnected.Invoke();

        SceneManager.LoadScene(0);

        //Debugger.Log("Pre shutdown", IsServer);
    }
    private void OnClientDisconnected(ulong clientId)
    {
        StopGame();
    }

    [ClientRpc]
    public void StartGameClientRpc()
    {
        OnConnected.Invoke();
    }

    public override void OnDestroy()
    {
        if (IsServer && NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }

        base.OnDestroy();
    }
}

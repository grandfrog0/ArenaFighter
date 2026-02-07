using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] GameObject playerPrefab;

    public void SetPlayerConfig(PlayerInfo info, ulong id)
    {
        GameObject player = Instantiate(playerPrefab);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id);
    }
}

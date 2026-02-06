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
    [SerializeField] GameObject playerPrefab;

    public void SetPlayerConfig(PlayerInfo info, ulong id)
    {
        GameObject player = Instantiate(playerPrefab);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id);
    }
}

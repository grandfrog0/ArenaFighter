using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;

public class RoundManager : NetworkBehaviour
{
    public static RoundManager Instance;

    public UnityEvent<int> OnRoundStart = new();
    public UnityEvent OnGameEnd = new();

    public UnityEvent<float> OnPlayer1HealthChanged = new();
    public UnityEvent<float> OnPlayer2HealthChanged = new();

    public UnityEvent<float> OnPlayer1TalismanRechargeChanged = new();
    public UnityEvent<float> OnPlayer2TalismanRechargeChanged = new();

    public UnityEvent<float> OnTimerPercentChanged = new();
    public UnityEvent<float> OnTimerChanged = new();
    [SerializeField] float roundTime = 60;
    [SerializeField] int maxRoundsCount = 3;

    private List<FighterEntity> _players = new();
    public List<Transform> playersPositions;
    private Coroutine _timer;

    public NetworkVariable<int> readyPlayers = new NetworkVariable<int>(0);

    private FighterEntity _player;
    private FighterEntity _enemy;

    [SerializeField] GameObject winScreen, loseScreen;
    [SerializeField] WinWindow winWindow;
    [SerializeField] LoseWindow loseWindow;
    [SerializeField] GameGuiWindow gameGui;
    [SerializeField] NetworkConnector networkConnector;

    private Coroutine _deathRoutine;

    [ServerRpc(RequireOwnership = false)]
    public void InitPlayerServerRpc(ulong clientId, SelectedPlayerData data)
    {
        FighterSettings fighter = PrefabBuffer.GetFighter(data.FighterId);
        FighterEntity entity = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<FighterEntity>();

        Debug.Log(clientId);

        PlayerController controller = entity.GetComponent<PlayerController>();
        controller.InitClientRpc(data);
        entity.InitClientRpc(data);

        readyPlayers.Value++;

        Debug.Log((readyPlayers.Value, NetworkManager.Singleton.ConnectedClients.Count));
        if (readyPlayers.Value == NetworkManager.Singleton.ConnectedClients.Count)
            StartGameClientRpc();
    }

    [ClientRpc]
    public void StartGameClientRpc()
    {
        _players = NetworkManager.Singleton.ConnectedClients.Select(x => x.Value.PlayerObject.GetComponent<FighterEntity>()).ToList();

        ulong playerId = NetworkManager.Singleton.LocalClientId;
        _player = _players.First(x => x.OwnerClientId == playerId);
        _player.OnDead.AddListener(OnPlayerDead);
        _player.OnHealthPercentChanged.AddListener(x => OnPlayer1HealthChanged.Invoke(x));
        _player.OnTalismanRechargeChangedUsed.AddListener(x => OnPlayer1TalismanRechargeChanged.Invoke(x));

        _enemy = _players.First(x => x.OwnerClientId != playerId);
        _enemy.OnDead.AddListener(OnPlayerDead);
        _enemy.OnHealthPercentChanged.AddListener(x => OnPlayer2HealthChanged.Invoke(x));
        _enemy.OnTalismanRechargeChangedUsed.AddListener(x => OnPlayer2TalismanRechargeChanged.Invoke(x));

        gameGui.InitPlayer(_player.PlayerData, _player);
        gameGui.InitEnemy(_enemy.PlayerData, _enemy);

        _timer = StartCoroutine(TimerRoutine());
    }

    public void StopGame()
    {
        if (_timer != null)
        {
            StopCoroutine(_timer);
            _timer = null;
        }

        if (IsOwner)
            EndGameServerRpc();
    }

    private IEnumerator TimerRoutine()
    {
        for (int round = 1; round <= maxRoundsCount; round++)
        {
            PrepareRound();
            OnRoundStart.Invoke(round);
            for (float t = roundTime; t >= 0; t -= Time.deltaTime)
            {
                OnTimerPercentChanged.Invoke(t / roundTime);
                OnTimerChanged.Invoke(Mathf.Round(t));
                yield return null;
            }
        }


        if (IsOwner)
            EndGameServerRpc();
    }

    [ServerRpc]
    private void EndGameServerRpc()
    {
        readyPlayers.Value = 0;
        EndGameClientRpc();
    }

    [ClientRpc]
    public void EndGameClientRpc()
    {
        foreach (FighterEntity p in _players)
        {
            p.OnHealthPercentChanged.RemoveAllListeners();
        }
        _players.Clear();

        if (_player && _enemy)
        {
            if (_player.Health > _enemy.Health)
                Win();
            else
                Lose();
        }

        _player = _enemy = null;

        networkConnector.Stop();

        OnGameEnd.Invoke();
    }

    public void Win()
    {
        winScreen.SetActive(true);
        winWindow.Win();
    }

    public void Lose()
    {
        loseScreen.SetActive(true);
        loseWindow.Lose();
    }

    private void PrepareRound()
    {
        int i = 0;
        foreach (FighterEntity p in _players)
        {
            p.Respawn();
            p.transform.position = playersPositions[i].position;
            Debug.Log(playersPositions[i].position);
            i++;
        }
    }

    public void DamagePlayer()
    {
        _player.TakeDamage(1000);
    }

    private void OnPlayerDead()
    {
        if (_deathRoutine == null)
            _deathRoutine = StartCoroutine(SkipRoundRoutine());
    }

    private IEnumerator SkipRoundRoutine()
    {
        yield return new WaitForSeconds(5);

        StopGame();

        _deathRoutine = null;
    }

    private void Awake()
    {
        Instance = this;
    }
}

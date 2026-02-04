using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class RoundManager : NetworkBehaviour
{
    public static RoundManager Instance;

    public UnityEvent<int> OnRoundStart = new();
    public UnityEvent OnGameEnd = new();

    public UnityEvent<float> OnPlayer1HealthChanged = new();

    public UnityEvent<float> OnTimerPercentChanged = new();
    public UnityEvent<float> OnTimerChanged = new();
    [SerializeField] float roundTime = 60;
    [SerializeField] int maxRoundsCount = 3;

    public List<FighterEntity> players;
    public List<Transform> playersPositions;
    private Coroutine _timer;

    [SerializeField] GameObject winScreen, loseScreen;
    [SerializeField] WinWindow winWindow;
    [SerializeField] LoseWindow loseWindow;
    [SerializeField] GameGuiWindow gameGui;
    [SerializeField] NetworkConnector networkConnector;

    private Coroutine _deathRoutine;

    public void InitPlayer(FighterSettings fighter, FightingTalisman talisman, StoreItem elixir)
    {
        FighterEntity entity = players.Find(x => x.OwnerClientId == OwnerClientId);
        if (entity)
        {
            PlayerController controller = entity.GetComponent<PlayerController>();
            controller.Init(fighter.Model);
            entity.Init(fighter, talisman, elixir);

            gameGui.Init(fighter, entity);
        }
        else Debug.LogError($"player is null! {OwnerClientId}, {players[0].OwnerClientId}");
    }

    public void StartGame()
    {
        _timer = StartCoroutine(TimerRoutine());
    }

    public void StopGame()
    {
        if (_timer != null)
        {
            StopCoroutine(_timer);
            _timer = null;
        }
        EndGame();
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
        EndGame();
        Win();
    }

    public void EndGame()
    {
        foreach (var p in players)
        {
            p.OnHealthPercentChanged.RemoveAllListeners();
        }
        players.Clear();

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
         for (int i = 0; i < players.Count; i++)
         {
            players[i].Respawn();
            players[i].transform.position = playersPositions[i].position;
         }
    }

    public void SetHealthTarget(FighterEntity entity)
    {
        entity.OnDead.AddListener(OnPlayerDead);
        entity.OnHealthPercentChanged.AddListener(x => OnPlayer1HealthChanged.Invoke(x));
    }

    public void DamagePlayer()
    {
        FighterEntity entity = players.Find(x => x.OwnerClientId == OwnerClientId);
        if (entity)
        {
            entity.TakeDamage(1000);
        }
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
        Lose();

        _deathRoutine = null;
    }

    private void Awake()
    {
        Instance = this;
    }
}

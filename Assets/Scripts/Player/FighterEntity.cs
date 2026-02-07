using System.Collections;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class FighterEntity : NetworkBehaviour
{
    public UnityEvent OnDead = new();
    public UnityEvent OnRespawn = new();
    public UnityEvent<bool> OnStunned = new();
    public UnityEvent<float> OnHealthPercentChanged = new();
    public UnityEvent<int> OnTalismanEffectUsed = new();
    public UnityEvent<float> OnTalismanRechargeChangedUsed = new();

    private float _maxHealth;
    private float _health;
    public float Health => _health;
    public float HealthPercent => _health / _maxHealth;

    private float _armKickDamage;
    private float _legKickDamage;
    private float _armKickSpeed;
    private float _legKickSpeed;
    private float _stunProbability;
    private Vector2 _stunTimeRange;

    private float _armKickDamageMultiplier = 1;
    private float _legKickDamageMultiplier = 1;

    public bool IsTalismanInCooldown { get; private set; } = false;
    public bool IsArmInCooldown { get; private set; } = false;
    public bool IsLegInCooldown { get; private set; } = false;

    private bool _isDead;

    private bool _isStunned;
    private Coroutine _stunRoutine;

    // items
    private FighterSettings _settings;
    private FightingTalisman _talisman;
    private FightingElixir _elixir;

    public SelectedPlayerData PlayerData { get; private set; }

    [SerializeField] AudioSource attackAudioSource;
    [SerializeField] AudioSource hurtAudioSource;
    private AudioClip _hurt1, _hurt2;

    private PlayerController _controller;

    private PlayerController _enemyController;

    [ClientRpc]
    public void InitClientRpc(SelectedPlayerData data)
    {
        PlayerData = data;
        _settings = PrefabBuffer.GetFighter(data.FighterId);
        _talisman = PrefabBuffer.GetTalisman(data.TalismanId);
        _elixir = PrefabBuffer.GetElixir(data.ElixirId);

        _health = _maxHealth = _settings.Health;
        _armKickDamage = _settings.ArmKickDamage;
        _legKickDamage = _settings.LegKickDamage;
        _armKickSpeed = _settings.ArmKickSpeed;
        _legKickSpeed = _settings.LegKickSpeed;
        _stunProbability = _settings.StunProbability;
        _stunTimeRange = _settings.StunTimeRange;

        _hurt1 = _settings.HurtSound1;
        _hurt2 = _settings.HurtSound2;

        _enemyController = NetworkManager.Singleton.ConnectedClients.First(
            x => x.Value.ClientId != OwnerClientId
        ).Value.PlayerObject.GetComponent<PlayerController>();

        if (_elixir)
        {
            StartCoroutine(ElixirRoutine());
        }
    }

    public void Respawn()
    {
        _health = _maxHealth;
        OnHealthPercentChanged.Invoke(HealthPercent);

        _isDead = false;
        OnRespawn.Invoke();
    }

    public void TakeDamage(float value)
    {
        TakeDamageServerRpc(value, true);
    }
    private void Heal(float value)
    {
        TakeDamageServerRpc(-value, false);
    }
    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(float value, bool useEffects)
    {
        TakeDamageClientRpc(value, useEffects);
    }
    [ClientRpc]
    private void TakeDamageClientRpc(float value, bool useEffects)
    {
        _health -= value;
        Debug.Log($"{name} got damage ({value}). Health left: {_health}");

        OnHealthPercentChanged.Invoke(HealthPercent);

        if (useEffects)
        {
            hurtAudioSource.clip = Random.Range(0, 2) == 0 ? _hurt1 : _hurt2;
            hurtAudioSource.Play();
        }

        if (_health <= 0 && !_isDead)
        {
            _isDead = true;
            OnDead.Invoke();
        }
    }
    public void Stun(float time)
    {
        _isStunned = true;
        Debug.Log("stunned!");
        OnStunned.Invoke(_isStunned);

        if (_stunRoutine != null)
            StopCoroutine(_stunRoutine);

        _stunRoutine = StartCoroutine(StunRoutine(time));
    }
    private IEnumerator StunRoutine(float stunTime)
    {
        yield return new WaitForSeconds(stunTime);

        Debug.Log("not stunned!");
        _isStunned = false;
        OnStunned.Invoke(_isStunned);
    }

    public void KickArm()
    {
        Attack(_armKickDamage * _armKickDamageMultiplier);
        StartCoroutine(ArmCooldownRoutine(_armKickSpeed));
    }
    public void KickLeg()
    {
        Attack(_legKickDamage * _legKickDamageMultiplier);
        StartCoroutine(LegCooldownRoutine(_legKickSpeed));
    }

    public void SpecialAttack()
    {
        if (!_talisman || IsTalismanInCooldown)
            return;

        if (IsOwner)
            SpecialAttackServerRpc();

        StartCoroutine(SpecialAttackRoutine());
    }
    [ServerRpc]
    private void SpecialAttackServerRpc()
        => SpecialAttackClientRpc();
    [ClientRpc]
    private void SpecialAttackClientRpc()
        => OnTalismanEffectUsed.Invoke(_talisman.Id);

    private IEnumerator SpecialAttackRoutine()
    {
        StartCoroutine(TalismanCooldownRoutine(_talisman.RechargeTime));

        switch (_talisman.Name)
        {
            case "FireTalisman":
                _armKickDamageMultiplier *= 1.5f;

                yield return new WaitForSeconds(_talisman.UseTime);

                _armKickDamageMultiplier /= 1.5f;
                break;

            case "LightningTalisman":
                _controller.SpeedMultiplier.Value *= 1.2f;
                _legKickDamageMultiplier *= 1.1f;

                yield return new WaitForSeconds(_talisman.UseTime);

                _controller.SpeedMultiplier.Value /= 1.2f;
                _legKickDamageMultiplier /= 1.1f;
                break;

            case "IceTalisman":
                _enemyController.SpeedMultiplier.Value *= 0.2f;

                yield return new WaitForSeconds(_talisman.UseTime);

                _enemyController.SpeedMultiplier.Value /= 0.2f;
                break;
        }
    }

    private IEnumerator TalismanCooldownRoutine(float time)
    {
        IsTalismanInCooldown = true;

        float updatePeriod = 0.5f;
        for (float t = 0; t < time * updatePeriod; t++)
        {
            OnTalismanRechargeChangedUsed.Invoke(t * updatePeriod / time);
            yield return new WaitForSeconds(updatePeriod);
        }

        IsTalismanInCooldown = false;
    }

    private void Attack(float damage)
    {
        var targets = Physics.OverlapSphere(transform.position, 3);
        foreach (var target in targets)
        {
            if (target.gameObject != gameObject && target.TryGetComponent(out FighterEntity enemy))
            {
                enemy.TakeDamage(damage);
                if (Random.Range(0, 100) < _stunProbability * 100)
                {
                    enemy.Stun(Random.Range(_stunTimeRange.x, _stunTimeRange.y));
                }

                attackAudioSource.Play();
                return;
            }
        }
    }

    private IEnumerator ArmCooldownRoutine(float time)
    {
        IsArmInCooldown = true;
        yield return new WaitForSeconds(time);
        IsArmInCooldown = false;
    }

    private IEnumerator LegCooldownRoutine(float time)
    {
        IsLegInCooldown = true;
        yield return new WaitForSeconds(time);
        IsLegInCooldown = false;
    }

    public override void OnNetworkSpawn()
    {
        _controller = GetComponent<PlayerController>();

        base.OnNetworkSpawn();
    }

    private IEnumerator ElixirRoutine()
    {
        switch (_elixir.Name)
        {
            // при достижении HP ниже 20% - начинает лечить игрока (по 1000 HP/сек в течении 5 секунд).
            // При повторном понижении HP до критического – вновь срабатывает
            case "SlowElixir":
                while (true)
                {
                    if (!_isDead && HealthPercent < 0.2f)
                        Heal(1000);

                    yield return new WaitForSeconds(5f);
                }

            // при критическом уровне здоровья - ниже 5% одномоментно повышает HP на 3000 единиц
            // (срабатывает 2 раза за бой, после действие прекращается)
            case "MomentalElixir":
                int usesLeft = 2;
                while (!_isDead && usesLeft > 0)
                {
                    yield return new WaitWhile(() => HealthPercent < 0.05f);
                    Heal(3000);
                    usesLeft--;
                }
                break;
        }
    }
}

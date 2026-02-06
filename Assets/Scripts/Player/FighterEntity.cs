using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class FighterEntity : NetworkBehaviour
{
    public UnityEvent OnDead = new();
    public UnityEvent OnRespawn = new();
    public UnityEvent<bool> OnStunned = new();
    public UnityEvent<float> OnHealthPercentChanged = new();
    public UnityEvent<FightingTalisman> OnTalismanEffectUsed = new();

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

    public bool IsArmInCooldown { get; private set; } = false;
    public bool IsLegInCooldown { get; private set; } = false;

    private bool _isDead;

    private bool _isStunned;
    private Coroutine _stunRoutine;

    // items
    public FighterSettings Settings { get; private set; }
    private FightingTalisman _talisman;
    private FightingElixir _elixir;

    [SerializeField] AudioSource attackAudioSource;
    [SerializeField] AudioSource hurtAudioSource;
    private AudioClip _hurt1, _hurt2;

    private PlayerController _controller;

    [ClientRpc]
    public void InitClientRpc(SelectedPlayerData data)
    {
        Settings = PrefabBuffer.GetFighter(data.FighterId);
        _talisman = PrefabBuffer.GetTalisman(data.TalismanId);
        _elixir = PrefabBuffer.GetElixir(data.ElixirId);

        _health = _maxHealth = Settings.Health;
        _armKickDamage = Settings.ArmKickDamage;
        _legKickDamage = Settings.LegKickDamage;
        _armKickSpeed = Settings.ArmKickSpeed;
        _legKickSpeed = Settings.LegKickSpeed;
        _stunProbability = Settings.StunProbability;
        _stunTimeRange = Settings.StunTimeRange;

        _hurt1 = Settings.HurtSound1;
        _hurt2 = Settings.HurtSound2;
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
        TakeDamageServerRpc(value);
    }
    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(float value)
    {
        TakeDamageClientRpc(value);
    }
    [ClientRpc]
    private void TakeDamageClientRpc(float value)
    {
        _health -= value;
        Debug.Log($"{name} got damage ({value}). Health left: {_health}");

        OnHealthPercentChanged.Invoke(HealthPercent);

        hurtAudioSource.clip = Random.Range(0, 2) == 0 ? _hurt1 : _hurt2;
        hurtAudioSource.Play();

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
        if (!_talisman)
            return;

        OnTalismanEffectUsed.Invoke(_talisman);
        StartCoroutine(SpecialAttackRoutine());
    }

    private IEnumerator SpecialAttackRoutine()
    {
        switch (_talisman.Name)
        {
            case "FireTalisman":
                _armKickDamageMultiplier = 1.5f;

                yield return new WaitForSeconds(_talisman.UseTime);

                _armKickDamageMultiplier = 1f;
                break;

            case "LightningTalisman":
                _controller.SpeedMultiplier = 1.2f;
                _legKickDamageMultiplier = 1.1f;

                yield return new WaitForSeconds(_talisman.UseTime);

                _controller.SpeedMultiplier = 1f;
                _legKickDamageMultiplier = 1f;
                break;

            case "IceTalisman":
                yield return new WaitForSeconds(_talisman.UseTime);
                break;
        }
    }

    private void Attack(float damage)
    {
        var targets = Physics.OverlapSphere(transform.position, 2);
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
}

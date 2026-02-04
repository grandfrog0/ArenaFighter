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
    public float HealthPercent => _health / _maxHealth;

    private float _armKickDamage;
    private float _legKickDamage;
    private float _armKickSpeed;
    private float _legKickSpeed;
    private float _stunProbability;
    private Vector2 _stunTimeRange;

    private float _armKickDamageMultiplier = 1;
    private float _legKickDamageMultiplier = 1;

    private bool _isDead;

    private bool _isStunned;
    private Coroutine _stunRoutine;

    // items
    private FightingTalisman _talisman;
    private StoreItem _elixir;

    [SerializeField] AudioSource attackAudioSource;
    [SerializeField] AudioSource hurtAudioSource;
    private AudioClip _hurt1, _hurt2;

    private PlayerController _controller;

    public void Init(FighterSettings _settings, FightingTalisman talisman, StoreItem elixir)
    {
        _health = _maxHealth = _settings.Health;
        _armKickDamage = _settings.ArmKickDamage;
        _legKickDamage = _settings.LegKickDamage;
        _armKickSpeed = _settings.ArmKickSpeed;
        _legKickSpeed = _settings.LegKickSpeed;
        _stunProbability = _settings.StunProbability;
        _stunTimeRange = _settings.StunTimeRange;

        _talisman = talisman;
        _elixir = elixir;

        _hurt1 = _settings.HurtSound1;
        _hurt2 = _settings.HurtSound2;
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
        OnStunned.Invoke(_isStunned);

        if (_stunRoutine != null)
            StopCoroutine(_stunRoutine);

        _stunRoutine = StartCoroutine(StunRoutine(time));
    }
    private IEnumerator StunRoutine(float stunTime)
    {
        yield return new WaitForSeconds(stunTime);

        _isStunned = false;
        OnStunned.Invoke(_isStunned);
    }

    public void KickArm()
    {
        Attack(_armKickDamage * _armKickDamageMultiplier);
    }
    public void KickLeg()
    {
        Attack(_legKickDamage * _legKickDamageMultiplier);
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
        attackAudioSource.Play();
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
                return;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        _controller = GetComponent<PlayerController>();

        RoundManager.Instance.players.Add(this);
        RoundManager.Instance.SetHealthTarget(this);
        base.OnNetworkSpawn();
    }
}

using System.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float baseSpeed = 5;
    [SerializeField] float runAddSpeed = 5;
    public float SpeedMultiplier = 1;
    public float MoveSpeed => (baseSpeed + (_isRunning ? runAddSpeed : 0)) * SpeedMultiplier;
    [Header("Action Settings")]
    [SerializeField] float jumpStrength = 10;
    [SerializeField] float rollStrength = 15;
    [SerializeField] float rollingTime = 0.5f;

    // logic
    private Vector3 _inputAxis;
    private bool _isRunning;
    private bool _canMove = true;
    private bool _isDead = false;
    private bool _isStunned = false;

    // components
    private Rigidbody _rb;
    private Animator _anim;
    private FighterEntity _entity;
    private CameraObserver _cam;

    // etc
    public GameObject Model;
    public Transform HeadPoint;

    [ClientRpc]
    public void InitClientRpc(SelectedPlayerData data)
    {
        _rb = GetComponent<Rigidbody>();
        _entity = GetComponent<FighterEntity>();

        Model = Instantiate(PrefabBuffer.GetFighter(data.FighterId).Model, transform);
        _anim = Model.GetComponent<Animator>();

        Debug.Log(OwnerClientId);
        if (IsOwner)
        {
            _cam = Camera.main.GetComponent<CameraObserver>();
            _cam.Target = HeadPoint;
            _cam.TargetModel = Model;
            _cam.RotateTransform = transform;
        }
    }

    private void Update()
    {
        if (!IsOwner || !_rb || _isDead)
            return;

        if (!_canMove || _isStunned)
        {
            SetInteger("Axis", 0);
            return;
        }

        HandleMove();
        HandleActions();
    }
    private void HandleMove()
    {
        _inputAxis = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        _isRunning = _inputAxis.z > 0 && Input.GetKey(KeyCode.LeftShift);

        Vector3 velocity = new Vector3(
            _inputAxis.x * MoveSpeed * Time.fixedDeltaTime * 100,
            _rb.linearVelocity.y,
            _inputAxis.z * MoveSpeed * Time.fixedDeltaTime * 100
        );

        _rb.linearVelocity = transform.rotation * velocity;

        // Animator
        int _animatorAxis = 
            (_isRunning ? 2 : 1) * 
            (_inputAxis.z != 0 ? (int)Mathf.Sign(_inputAxis.z) : _inputAxis.x != 0 ? -1 : 0);

        SetInteger("Axis", _animatorAxis);
    }
    private void HandleActions()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Roll();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            SpecialAttack();
        }
        else if (Input.GetMouseButtonDown(0))
        {
            KickArm();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            KickLeg();
        }
    }

    private void Jump()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, 1))
            return;

        _rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
        SetTrigger("OnJump");
    }

    private void Roll()
    {
        StartCoroutine(RollRoutine());
        SetTrigger("OnRoll");
    }
    private IEnumerator RollRoutine()
    {
        _canMove = false;
        _rb.AddForce((_inputAxis != Vector3.zero ? transform.rotation * _inputAxis : transform.forward).normalized * rollStrength, ForceMode.Impulse);
        SetBoolean("CanMove", false);

        yield return new WaitForSeconds(rollingTime);
        _canMove = true;
        SetBoolean("CanMove", true);
    }
    private void SpecialAttack()
    {
        _entity.SpecialAttack();
    }
    private void KickArm()
    {
        if (_entity.IsArmInCooldown)
            return;

        SetTrigger("OnArmKick");
        _entity.KickArm();
    }
    private void KickLeg()
    {
        if (_entity.IsLegInCooldown)
            return;

        SetTrigger("OnLegKick");
        _entity.KickLeg();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetDeadServerRpc(bool value)
    {
        SetDeadClientRpc(value);
    }

    [ClientRpc]
    private void SetDeadClientRpc(bool value)
    {
        _isDead = value;
        SetBoolean("IsDead", value);

        if (_cam)
            _cam.enabled = !value;
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnStunnedServerRpc(bool value)
        => OnStunnedClientRpc(value);
    [ClientRpc]
    public void OnStunnedClientRpc(bool value)
        => _isStunned = value;

    // Animator network scripts
    private void SetInteger(string key, int value)
        => SetIntegerServerRpc(key, value);
    [ServerRpc]
    private void SetIntegerServerRpc(string key, int value)
        => SetIntegerClientRpc(key, value);
    [ClientRpc]
    private void SetIntegerClientRpc(string key, int value)
        => _anim?.SetInteger(key, value);

    private void SetBoolean(string key, bool value)
        => SetBooleanServerRpc(key, value);
    [ServerRpc(RequireOwnership = false)]
    private void SetBooleanServerRpc(string key, bool value)
        => SetBooleanClientRpc(key, value);
    [ClientRpc]
    private void SetBooleanClientRpc(string key, bool value)
        => _anim?.SetBool(key, value);

    private void SetTrigger(string key)
        => SetTriggerServerRpc(key);
    [ServerRpc]
    private void SetTriggerServerRpc(string key)
        => SetTriggerClientRpc(key);
    [ClientRpc]
    private void SetTriggerClientRpc(string key)
    {
        _anim?.SetTrigger(key);
    }
}

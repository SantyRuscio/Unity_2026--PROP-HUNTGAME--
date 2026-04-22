using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;
using Fusion.Addons.Physics;
using Unity.Mathematics;
using System;

public class Player : NetworkBehaviour
{

    [Header("Movement")]
    [SerializeField] private InputActionReference _moveInputReference;
    [SerializeField] private float _speed;
    [SerializeField] private float _jumpForce;

    [Header("Physics")]
    [SerializeField] private NetworkRigidbody3D _netRb;

    [Header("Ground Check")]
    [SerializeField] private float _groundCheckDistance = 1.1f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Roles del Parcial")]
    public bool isHunter;
    private bool _shootPressed;

    [Header("Prop Hunt Mechanic")]
    [Networked] public int CurrentPropID { get; set; }
    private bool _transformPressed;

    [Header("Match Timer")]
    [Networked] public TickTimer MatchTimer { get; set; }
    [SerializeField] private float _matchTime = 60f; 

    private bool _jumpPressed;
    private bool _isGrounded;
    public bool IsGrounded => _isGrounded;

    public event Action<float> OnMovement;
    public event Action OnJump;
    public event Action<bool> OnGroundedChanged;

    public event Action OnTaunt;

    public override void Spawned()
    {
        Debug.Log($"Spawned {Object.HasStateAuthority}");

        if (Object.HasStateAuthority)
        {
            GetComponentInChildren<Renderer>().material.color = Color.blue;
            _netRb = GetComponent<NetworkRigidbody3D>();

            Camera.main.GetComponent<CameraFollow>().SetTarget(transform);
        }
        if (Object.HasStateAuthority && isHunter)
        {
            MatchTimer = TickTimer.CreateFromSeconds(Runner, _matchTime);
        }
        else
        {
            GetComponentInChildren<Renderer>().material.color = Color.red;
        }
    }

    public override void Render()
    {
        if (!Object.HasStateAuthority) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            _jumpPressed = true;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (isHunter) _shootPressed = true;
            else _transformPressed = true;
        }

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            RPC_PlayTaunt();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayTaunt()
    {
        OnTaunt?.Invoke();
    }

    public override void FixedUpdateNetwork()
    {
        if (_transformPressed)
        {
            TryTransform();
            _transformPressed = false;
        }

        if (isHunter && _shootPressed)
        {
            TryShoot();
            _shootPressed = false;
        }

        if (MatchTimer.IsRunning && MatchTimer.Expired(Runner))
        {
            RPC_NotifyGameEnd("¡VICTORIA DEL PROP! Sobrevivió el tiempo límite.");

            if (Object.HasStateAuthority)
            {
                MatchTimer = TickTimer.None;
            }
        }

    }

    void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        bool wasGrounded = _isGrounded;

        _isGrounded = Physics.Raycast(
            origin,
            Vector3.down,
            _groundCheckDistance,
            _groundLayer
        );

        Debug.DrawRay(origin, Vector3.down * _groundCheckDistance, Color.red);

        if (wasGrounded != _isGrounded)
        {
            OnGroundedChanged?.Invoke(_isGrounded);
        }
    }

    // Movimiento
    void Movement()
    {
        var moveInput = _moveInputReference.action.ReadValue<Vector2>();

        var velocity = _netRb.Rigidbody.linearVelocity;

        velocity.x = moveInput.x * _speed;
        velocity.z = moveInput.y * _speed;

        _netRb.Rigidbody.linearVelocity = velocity;

        Vector3 dir = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        if (dir != Vector3.zero)
        {
            transform.forward = -dir;
        }

        OnMovement?.Invoke(moveInput.magnitude);
    }

    void Jump()
    {
        _netRb.Rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
        OnJump?.Invoke();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        Debug.Log("Despawned");
    }

    void TryTransform()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            PropData propData = hit.collider.GetComponent<PropData>();

            if (propData != null)
            {
                CurrentPropID = propData.PropID;
            }
        }
    }

    void TryShoot()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, 15f))
        {
            Player hitPlayer = hit.collider.GetComponentInParent<Player>();

            if (hitPlayer != null && !hitPlayer.isHunter)
            {
                RPC_NotifyGameEnd("¡VICTORIA DEL HUNTER! El Prop fue descubierto.");
            }
        }
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NotifyGameEnd(string message)
    {
        Debug.LogWarning(message);
        // acá se puede poner un cartel para avisar en la ui que ganó el hunter
    }
}
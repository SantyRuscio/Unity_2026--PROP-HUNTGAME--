using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;
using Fusion.Addons.Physics;
using System;

public class Player : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private InputActionReference _moveInputReference;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _jumpForce = 7f;

    [Header("Physics")]
    [SerializeField] private NetworkRigidbody3D _netRb;

    [Header("Ground Check")]
    [SerializeField] private float _groundCheckDistance = 1.1f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Roles del Parcial")]
    [Tooltip("Tildar solo en el prefab del Hunter")]
    public bool isHunter;
    private bool _shootPressed;

    [Header("Prop Hunt Mechanic")]
    [Networked] public int CurrentPropID { get; set; }
    [SerializeField] private int _maxProps = 2;
    private bool _transformPressed;

    [Header("Match Timer")]
    [Networked] public TickTimer MatchTimer { get; set; }
    [Networked] public TickTimer HideTimer { get; set; } 
    [SerializeField] private float _matchTime = 60f;
    [SerializeField] private float _hideTime = 15f;

    private bool _jumpPressed;
    private bool _isGrounded;
    public bool IsGrounded => _isGrounded;

    public event Action<float> OnMovement;
    public event Action OnJump;
    public event Action<bool> OnGroundedChanged;
    public event Action OnTaunt;
    public event Action OnShoot;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {

            _netRb = GetComponent<NetworkRigidbody3D>();
            Camera.main.GetComponent<CameraFollow>().SetTarget(transform);

            if (isHunter)
            {
                HideTimer = TickTimer.CreateFromSeconds(Runner, _hideTime);
            }
        }
        else
        {

        }
    }

    private void SetPlayerColor(Color teamColor)
    {
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer ren in allRenderers)
        {
            foreach (Material mat in ren.materials)
            {
                mat.color = teamColor;
            }
        }
    }

    public override void Render()
    {
        if (!Object.HasStateAuthority) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame) _jumpPressed = true;

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
        if (Runner.SessionInfo.PlayerCount < 2) return;

        if (isHunter && Object.HasStateAuthority)
        {
            if (HideTimer.IsRunning && HideTimer.Expired(Runner))
            {
                HideTimer = TickTimer.None; 
                MatchTimer = TickTimer.CreateFromSeconds(Runner, _matchTime);

                RPC_NotifyGameEnd("¡CUIDADO! El Hunter ha sido liberado.");
            }
        }


        bool hunterIsFrozen = isHunter && HideTimer.IsRunning;

        // 3. ACCIONES
        if (_transformPressed && !isHunter) 
        {
            CycleProp();
            _transformPressed = false;
        }

        if (isHunter && _shootPressed && !hunterIsFrozen) 
        {
            TryShoot();
            _shootPressed = false;
        }

        if (MatchTimer.IsRunning && MatchTimer.Expired(Runner))
        {
            RPC_NotifyGameEnd("¡TIEMPO AGOTADO! Victoria de los Props.");
            if (Object.HasStateAuthority) MatchTimer = TickTimer.None;
        }

        if (!Object.HasStateAuthority) return;

        CheckGround();

        if (!hunterIsFrozen)
        {

            Movement();

            if (_jumpPressed)
            {
                if (_isGrounded) Jump();
                _jumpPressed = false;
            }
        }
        else
        {
            _netRb.Rigidbody.linearVelocity = new Vector3(0, _netRb.Rigidbody.linearVelocity.y, 0);
            _jumpPressed = false;
        }
    }

    void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        bool wasGrounded = _isGrounded;

        _isGrounded = Physics.Raycast(origin, Vector3.down, _groundCheckDistance, _groundLayer);

        if (wasGrounded != _isGrounded) OnGroundedChanged?.Invoke(_isGrounded);
    }

    void Movement()
    {
        var moveInput = _moveInputReference.action.ReadValue<Vector2>();

        Transform cam = Camera.main.transform;

        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = forward * moveInput.y + right * moveInput.x;

        var velocity = _netRb.Rigidbody.linearVelocity;
        velocity.x = moveDir.x * _speed;
        velocity.z = moveDir.z * _speed;
        _netRb.Rigidbody.linearVelocity = velocity;

        CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow != null)
        {
            float yaw = camFollow.GetYaw();
            transform.rotation = Quaternion.Euler(0, yaw, 0);
        }

        OnMovement?.Invoke(moveInput.magnitude);
    }

    void Jump()
    {
        RPC_PlayJump();

        _netRb.Rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
        OnJump?.Invoke();
    }

    void CycleProp()
    {
        CurrentPropID++;

        if (CurrentPropID > _maxProps)
        {
            CurrentPropID = 1;
        }
    }

    void TryShoot()
    {
        RPC_PlayShoot();

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        Debug.DrawRay(ray.origin, ray.direction * 50f, Color.red, 2f);

        if (Physics.Raycast(ray, out RaycastHit hit, 50f))
        {
            Player hitPlayer = hit.transform.root.GetComponent<Player>();

            if (hitPlayer != null && !hitPlayer.isHunter)
            {
                RPC_NotifyGameEnd("¡PROP ENCONTRADO! Victoria del Hunter.");
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NotifyGameEnd(string message)
    {
        Debug.LogWarning(message);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayShoot()
    {
        OnShoot?.Invoke();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayJump()
    {
        OnJump?.Invoke();
    }
}
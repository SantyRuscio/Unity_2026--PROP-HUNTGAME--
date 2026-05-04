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

    [Header("Roles")]
    public bool isHunter;
    private bool _shootPressed;

    [Header("Prop Hunt")]
    [Networked] public int CurrentPropID { get; set; }
    [SerializeField] private int _maxProps = 15;
    private bool _transformPressed;

    [Header("LOCK (Freeze Prop)")]
    private bool _lockPressed;
    [Networked] private bool _isLocked { get; set; }

    [Header("Timers")]
    [Networked] public TickTimer MatchTimer { get; set; }
    [Networked] public TickTimer HideTimer { get; set; }
    [SerializeField] private float _matchTime = 60f;
    [SerializeField] private float _hideTime = 15f;

    [Header("Taunt (Silbido)")]
    [SerializeField] private AudioSource tauntAudio;
    [SerializeField] private float tauntCooldown = 5f;
    private float lastTauntTime = -999f;

    private bool _jumpPressed;
    private bool _isGrounded;

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
    }

    public override void Render()
    {
        if (!Object.HasStateAuthority) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            _jumpPressed = true;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (isHunter) _shootPressed = true;
            else _transformPressed = true;
        }

        // TAUNT
        if (Keyboard.current.tKey.wasPressedThisFrame && !isHunter)
        {
            if (Time.time >= lastTauntTime + tauntCooldown)
            {
                lastTauntTime = Time.time;
                RPC_PlayTaunt();
            }
        }

        // LOCK PROP (F)
        if (Keyboard.current.fKey.wasPressedThisFrame && !isHunter)
        {
            _lockPressed = true;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayTaunt()
    {
        OnTaunt?.Invoke();

        if (tauntAudio != null)
        {
            tauntAudio.Play();
        }
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
                Debug.Log("Hunter liberado");
            }
        }

        bool hunterIsFrozen = isHunter && HideTimer.IsRunning;

        // TOGGLE LOCK
        if (_lockPressed && !isHunter)
        {
            _isLocked = !_isLocked;
            _lockPressed = false;
        }

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
            RPC_NotifyGameEnd("¡TIEMPO AGOTADO!\nGanan los Props");
            if (Object.HasStateAuthority) MatchTimer = TickTimer.None;
        }

        if (!Object.HasStateAuthority) return;

        CheckGround();

        if (!hunterIsFrozen && !_isLocked)
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
            // FREEZE TOTAL
            _netRb.Rigidbody.linearVelocity = Vector3.zero;
            _netRb.Rigidbody.angularVelocity = Vector3.zero;
            _jumpPressed = false;
        }
    }

    void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        bool wasGrounded = _isGrounded;

        _isGrounded = Physics.Raycast(origin, Vector3.down, _groundCheckDistance, _groundLayer);

        if (wasGrounded != _isGrounded)
            OnGroundedChanged?.Invoke(_isGrounded);
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

        float yaw = Camera.main.GetComponent<CameraFollow>().GetYaw();

        // BLOQUEAR ROTACIÓN SI ESTÁ LOCKED
        if (!_isLocked)
        {
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
            CurrentPropID = 1;
    }

    void TryShoot()
    {
        RPC_PlayShoot();

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit[] hits = Physics.SphereCastAll(ray, 0.5f, 50f);

        foreach (var hit in hits)
        {
            Player p = hit.transform.root.GetComponent<Player>();

            if (p != null && !p.isHunter)
            {
                RPC_NotifyGameEnd("¡PROP ENCONTRADO!\nGana el Hunter");
                return;
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NotifyGameEnd(string msg)
    {
        MatchTimer = TickTimer.None;
        HideTimer = TickTimer.None;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameOver(msg);
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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_RestartGame()
    {
        if (UIManager.Instance != null && UIManager.Instance.gameOverPanel != null)
        {
            UIManager.Instance.gameOverPanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        MatchTimer = TickTimer.None;
        HideTimer = TickTimer.None;

        string spawnName = isHunter ? "Spawn_Player1" : "Spawn_Player2";
        GameObject spawn = GameObject.Find(spawnName);

        if (spawn != null)
        {
            _netRb.Rigidbody.linearVelocity = Vector3.zero;
            _netRb.Rigidbody.angularVelocity = Vector3.zero;

            transform.position = spawn.transform.position;
            transform.rotation = spawn.transform.rotation;
        }
    }
}
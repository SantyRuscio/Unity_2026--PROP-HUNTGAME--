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

    private bool _jumpPressed;
    private bool _isGrounded;
    public bool IsGrounded => _isGrounded;

    public event Action<float> OnMovement;
    public event Action OnJump;
    public event Action<bool> OnGroundedChanged;

    public override void Spawned()
    {
        Debug.Log($"Spawned {Object.HasStateAuthority}");

        if (Object.HasStateAuthority)
        {
            GetComponentInChildren<Renderer>().material.color = Color.blue;
            _netRb = GetComponent<NetworkRigidbody3D>();

            Camera.main.GetComponent<CameraFollow>().SetTarget(transform);
        }
        else
        {
            GetComponentInChildren<Renderer>().material.color = Color.red;
            enabled = false;
        }
    }

    public override void Render()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            _jumpPressed = true;
        }
    }

    public override void FixedUpdateNetwork()
    {
        CheckGround();
        Movement();

        if (_jumpPressed)
        {
            if (_isGrounded)
            {
                Jump();
            }

            _jumpPressed = false;
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
            transform.forward = -dir; // corregido por tu modelo
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
}
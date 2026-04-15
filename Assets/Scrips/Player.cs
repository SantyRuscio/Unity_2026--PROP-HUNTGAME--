using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;
using Fusion.Addons.Physics;
using Unity.Mathematics;

public class Player : NetworkBehaviour
{

    [SerializeField] InputActionReference _moveInputReference;
    [SerializeField] private float _speed;
    [SerializeField] private float _jumpForce;

    [SerializeField] NetworkRigidbody3D _netRb;

    private bool _jumpPressed;

    //Es como el awake-start pero se ejecuta cuando el objeto es metido a la red (runner.spawn)
    // Se ejecuta en todos los clientes
    public override void Spawned()
    {
        Debug.Log($"Spawned {Object.HasStateAuthority}");

        if (Object.HasStateAuthority)
        {
            GetComponentInChildren<Renderer>().material.color = Color.blue;
            _netRb = GetComponent<NetworkRigidbody3D>();
        }
        else
        {
            GetComponentInChildren<Renderer>().material.color = Color.red;
            enabled = false;
        }
    }

    //Update
    // Se ejecuta en todos los clientes
    public override void Render()
    {
        Debug.Log($"Render{Object.HasStateAuthority}");

        if(Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            _jumpPressed = true;    
        }
    }

    //es el update o una especie de fixUpdate que se ejecuta cada vez que la red se actualiza
    // Se ejecuta solo en los que tienen autoridad de estado o iNPUT.
    public override void FixedUpdateNetwork()
    {
        Debug.Log($"FixedUpdateNetwork {Object.HasStateAuthority}" );

        Movement();

        if (_jumpPressed)
        {
            _jumpPressed = false;
            Jump(); 
        }
    }

    void Movement()
    {
        var moveInpuT = _moveInputReference.action.ReadValue<Vector2>();

        if(moveInpuT.x != 0)
        {
            transform.right = Vector3.right * Mathf.Sign(moveInpuT.x);
        }

        //Movernos de izq a der en base de moveInput
        //transform.position += Vector3.right * moveInpuT.x * (_speed * Time.deltaTime);

        _netRb.Rigidbody.linearVelocity += Vector3.right * (moveInpuT.x * 10 * _speed * Time.deltaTime);

        if (Mathf.Abs (_netRb.Rigidbody.linearVelocity.x) <= _speed) return;

        var newVelocity = _netRb.Rigidbody.linearVelocity;
        newVelocity.x = moveInpuT.x * _speed;
        _netRb.Rigidbody.linearVelocity = newVelocity;
    }

    void Jump()
    {
        _netRb.Rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
    }

    //es como el OnDestroy pero se ejecuta cuando el objeto es eliminado de la red (runner.despawn)
    // Se ejecuta en todos los clientes
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        Debug.Log("Despawned" );
    }
}

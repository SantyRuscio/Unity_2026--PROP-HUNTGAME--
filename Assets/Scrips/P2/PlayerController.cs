using Fusion;
using UnityEngine;

[RequireComponent(typeof(PlayerMov))]
[RequireComponent(typeof(PlayerWeapon))]
public class PlayerController : NetworkBehaviour
{
    private PlayerMov _playerMov;
    private PlayerWeapon _weapon;
    private MovementAnimation _anim;

    [SerializeField] private float sensitivity = 0.1f;

    private void Awake()
    {
        _playerMov = GetComponent<PlayerMov>();
        _weapon = GetComponent<PlayerWeapon>();
        _anim = GetComponentInChildren<MovementAnimation>();
    }
    public override void FixedUpdateNetwork()
    {
        if (Runner.SessionInfo.PlayerCount < 2) return;
        if (!GetInput(out NetworkInputData inputs)) return;

        // Rotar el player según el mouse
        transform.Rotate(Vector3.up * inputs.lookYaw * sensitivity);

        // Mover con WASD (x = strafe, z = adelante/atrás)
        Vector3 dir = new Vector3(inputs.moveAxis.x, 0, inputs.moveAxis.y);
        _playerMov.Move(dir);

        if (inputs.Buttons.IsSet(ButtonTypes.Jump))
            _playerMov.Jump();

        if (inputs.Buttons.IsSet(ButtonTypes.Shot))
        {
            _weapon.ShootRaycast();
            _anim.TriggerShoot();
        }
    }
}

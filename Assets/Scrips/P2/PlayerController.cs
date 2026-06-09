using Fusion;
using UnityEngine;

[RequireComponent(typeof(PlayerMov))]
[RequireComponent(typeof(PlayerWeapon))]
public class PlayerController : NetworkBehaviour
{
    private PlayerMov _playerMov;
    private PlayerWeapon _weapon;

    private void Awake()
    {
        _playerMov = GetComponent<PlayerMov>();

        _weapon = GetComponent<PlayerWeapon>();
    }
    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData inputs)) return;

        _playerMov.Move(Vector3.right * inputs.xAxi);

    

        if (inputs.Buttons.IsSet(ButtonTypes.Jump))
        {
            _playerMov.Jump();
        }

        if (inputs.Buttons.IsSet(ButtonTypes.Shot))
        {
            // _weapon.ShootGameObject();
            _weapon.ShootRaycast(); 

        }
    }
}

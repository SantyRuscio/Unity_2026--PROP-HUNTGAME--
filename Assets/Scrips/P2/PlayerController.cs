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

    [Networked] public NetworkBool IsHunter { get; set; }
    [Networked] public int CurrentPropID { get; set; }

    // Nueva variable en red para saber si el Prop está congelado en el lugar
    [Networked] public NetworkBool IsFrozen { get; set; }

    private int _maxProps = 3;

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

        // Si está congelado, no le permitimos rotar la cámara al personaje
        if (!IsFrozen)
        {
            transform.Rotate(Vector3.up * inputs.lookYaw * sensitivity);
        }

        Vector3 dir = new Vector3(inputs.moveAxis.x, 0, inputs.moveAxis.y);

        // Si está congelado, mandamos un vector cero para que no se mueva en absoluto
        if (IsFrozen)
        {
            _playerMov.Move(Vector3.zero);
        }
        else
        {
            _playerMov.Move(dir);

            if (inputs.Buttons.IsSet(ButtonTypes.Jump))
                _playerMov.Jump();
        }

        if (IsHunter)
        {
            if (inputs.Buttons.IsSet(ButtonTypes.Shot))
            {
                _weapon.ShootRaycast();
                if (_anim != null) _anim.TriggerShoot();
            }
        }
        else
        {
            if (inputs.Buttons.IsSet(ButtonTypes.Transform))
            {
                CycleProp();
            }

            // Si es un Prop y presiona la E, toggleamos el estado de congelación
            if (inputs.Buttons.IsSet(ButtonTypes.Freeze))
            {
                IsFrozen = !IsFrozen;
            }
        }
    }

    void CycleProp()
    {
        CurrentPropID++;
        if (CurrentPropID > _maxProps)
            CurrentPropID = 0;
    }
}
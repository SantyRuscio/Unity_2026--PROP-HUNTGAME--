using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalInputs : NetworkBehaviour
{
   public static LocalInputs Instance { get; private set; }

    private NetworkInputData _inputData;

    [SerializeField] InputActionReference _moveActionReference;

    bool _isJumpPressed;
    bool _isFirePressed;


    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            _inputData = new NetworkInputData();
            Instance = this;
            return; 
        }

        enabled = false;
    }

    public void Update()
    {
        // if (Keyboard.current.spaceKey.wasPressedThisFrame)
        // {
        //     _isJumpPressed = true;
        // }
        // if (Keyboard.current.wKey.wasPressedThisFrame)
        // {
        //     _isFirePressed = true;
        // }

        _isFirePressed |= Mouse.current.leftButton.wasPressedThisFrame;
        _isJumpPressed |= Keyboard.current.spaceKey.wasPressedThisFrame;

    }

    public NetworkInputData GetInputData()
    {
        _inputData.moveAxis = _moveActionReference.action.ReadValue<Vector2>();

        // rotación con mouse
        _inputData.lookYaw = Mouse.current.delta.ReadValue().x;

        _inputData.Buttons.Set(ButtonTypes.Jump, _isJumpPressed);
        _isJumpPressed = false;

        _inputData.Buttons.Set(ButtonTypes.Shot, _isFirePressed);
        _isFirePressed = false;

        return _inputData;
    }

}
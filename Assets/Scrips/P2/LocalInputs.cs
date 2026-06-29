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
    bool _isTransformPressed;
    bool _isFreezePressed;
    bool _isWhistlePressed;

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
        _isFirePressed |= Mouse.current.leftButton.wasPressedThisFrame;
        _isJumpPressed |= Keyboard.current.spaceKey.wasPressedThisFrame;

        _isTransformPressed |= Mouse.current.rightButton.wasPressedThisFrame;

        if (Keyboard.current != null)
        {
            _isFreezePressed |= Keyboard.current.eKey.wasPressedThisFrame;

            _isWhistlePressed |= Keyboard.current.fKey.wasPressedThisFrame;
        }
    }

    public NetworkInputData GetInputData()
    {
        _inputData.moveAxis = _moveActionReference.action.ReadValue<Vector2>();

        _inputData.lookYaw = Mouse.current.delta.ReadValue().x;
        _inputData.lookPitch = Mouse.current.delta.ReadValue().y; 

        _inputData.Buttons.Set(ButtonTypes.Jump, _isJumpPressed);
        _isJumpPressed = false;

        _inputData.Buttons.Set(ButtonTypes.Shot, _isFirePressed);
        _isFirePressed = false;

        _inputData.Buttons.Set(ButtonTypes.Transform, _isTransformPressed);
        _isTransformPressed = false;

        _inputData.Buttons.Set(ButtonTypes.Freeze, _isFreezePressed);
        _isFreezePressed = false;

        _inputData.Buttons.Set(ButtonTypes.Whistle, _isWhistlePressed);
        _isWhistlePressed = false;

        return _inputData;
    }
}
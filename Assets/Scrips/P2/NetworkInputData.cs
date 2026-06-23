using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public float xAxi;
    public Vector2 moveAxis; // x e y del joystick/WASD
    public float lookYaw;    // rotaci�n horizontal del mouse
    public const byte MouseButton0 = 1;

    //public NetworkBool isShotPressed;

    public NetworkButtons Buttons;
}

public enum ButtonTypes
{
    Jump=0,
    Shot =1,
    Transform = 2,
    Freeze = 3
}

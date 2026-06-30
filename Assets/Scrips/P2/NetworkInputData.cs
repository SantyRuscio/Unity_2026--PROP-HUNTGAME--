using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public float xAxi;
    public Vector2 moveAxis;
    public float lookYaw;  
    public float lookPitch;  
    public const byte MouseButton0 = 1;

    //public NetworkBool isShotPressed;

    public NetworkButtons Buttons;
}

public enum ButtonTypes
{
    Jump = 0,
    Shot = 1,
    Transform = 2,
    Freeze = 3,
    Whistle = 4
}
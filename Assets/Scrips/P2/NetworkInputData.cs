using UnityEngine;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    public float xAxi;
    public const byte MouseButton0 = 1;

    //public NetworkBool isShotPressed;

    public NetworkButtons Buttons;
}

public enum ButtonTypes
{
    Jump=0,
    Shot =1
}

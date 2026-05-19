using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData inputs)) return;

        transform.position += Vector3.right * (inputs.xAxi * 3 * Time.deltaTime);

        if (inputs.Buttons.IsSet(ButtonTypes.Jump))
        {
            //JUMP
        }

        if (inputs.Buttons.IsSet(ButtonTypes.Shot))
        {
            //FIRE
        }
    }
}

using Fusion;
using System;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerMov : NetworkCharacterController
{
    public event Action<float> OnMovement;
    public override void Move(Vector3 direction)
    {
        var deltaTime = Runner.DeltaTime;
        var previousPos = transform.position;
        var moveVelocity = Velocity;

        direction = direction.normalized;

        if (Grounded && moveVelocity.y < 0)
            moveVelocity.y = 0f;

        moveVelocity.y += gravity * Runner.DeltaTime;

        var horizontalVel = new Vector3(moveVelocity.x, 0, moveVelocity.z);

        if (direction == default)
        {
            horizontalVel = Vector3.Lerp(horizontalVel, Vector3.zero, braking * deltaTime);
        }
        else
        {
            Vector3 worldDir = transform.TransformDirection(direction);
            horizontalVel = Vector3.ClampMagnitude(
                horizontalVel + worldDir * acceleration * deltaTime, maxSpeed);
        }

        moveVelocity.x = horizontalVel.x;
        moveVelocity.z = horizontalVel.z;

        _controller.Move(moveVelocity * deltaTime);

        Velocity = (transform.position - previousPos) * Runner.TickRate;
        Grounded = _controller.isGrounded;

        OnMovement?.Invoke(new Vector2(Velocity.x, Velocity.z).magnitude);
    }
}

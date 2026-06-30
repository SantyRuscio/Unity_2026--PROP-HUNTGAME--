using Fusion;
using UnityEngine;

public class MovementAnimation : NetworkBehaviour
{
    [SerializeField] private NetworkMecanimAnimator _mecanim;
    [SerializeField] private PlayerMov _movement;

    private void Awake()
    {
        _movement.OnMovement += UpdateAnimation;
    }

    public override void Spawned()
    {
       // // Solo correr animaciones localmente
       // if (!Object.HasInputAuthority)
       //     _mecanim.enabled = false;
    }

    private void UpdateAnimation(float velocity)
    {
        var animator = _mecanim.Animator;

        animator.SetFloat("speed", velocity);

        animator.SetBool("isGrounded", _movement.Grounded);

        if (!_movement.Grounded)
            animator.SetBool("jump", true);
        else
            animator.SetBool("jump", false);
    }
    public void TriggerShoot()
    {
        _mecanim.Animator.SetTrigger("shoot");
    }
}

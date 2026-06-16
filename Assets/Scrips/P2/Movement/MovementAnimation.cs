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

        // Velocidad para Idle <-> Run
        animator.SetFloat("speed", velocity);

        // Grounded
        animator.SetBool("isGrounded", _movement.Grounded);

        // Jump (trigger, solo cuando despega)
        if (!_movement.Grounded)
            animator.SetBool("jump", true);
        else
            animator.SetBool("jump", false);
    }

    // Llamar esto desde PlayerController cuando dispara
    public void TriggerShoot()
    {
        _mecanim.Animator.SetTrigger("shoot");
    }
}

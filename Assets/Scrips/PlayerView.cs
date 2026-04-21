using Fusion;
using UnityEngine;
public class PlayerView : MonoBehaviour
{
    [SerializeField] private NetworkMecanimAnimator _mecanim;
    [SerializeField] private ParticleSystem _fireParticles;

    private Player _player;

    private void OnEnable()
    {
        _player = GetComponentInParent<Player>();

        if (_player != null)
        {
            _player.OnMovement += HandleMovement;
            _player.OnJump += HandleJump;
            _player.OnGroundedChanged += HandleGrounded;
        }
    }

    private void OnDisable()
    {
        if (_player != null)
        {
            _player.OnMovement -= HandleMovement;
            _player.OnJump -= HandleJump;
            _player.OnGroundedChanged -= HandleGrounded;
        }
    }

    void HandleMovement(float speed)
    {
        _mecanim.Animator.SetFloat("speed", speed);
    }

    void HandleJump()
    {
        _mecanim.Animator.SetTrigger("jump");
    }

    void HandleGrounded(bool grounded)
    {
        _mecanim.Animator.SetBool("isGrounded", grounded);
    }

    void ShotParticles()
    {
        _fireParticles.Play();
    }
}
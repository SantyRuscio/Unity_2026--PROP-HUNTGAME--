using Fusion;
using UnityEngine;

public class MovementAnimation : MonoBehaviour
{
    [SerializeField] private NetworkMecanimAnimator _mecanim;
    [SerializeField] private PlayerMov _movement;

    private void Awake()
    {
        _movement.OnMovement += UpdateAnimation;
    }

    private void UpdateAnimation(float xVelocity)
    {
        _mecanim.Animator.SetFloat("Runn", Mathf.Abs(xVelocity)); //agregar coorrecatament el bool 
    }
}

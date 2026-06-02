using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    //Variables requeridas: Damage, Tiempo de vida, Fuerza de movimiento

    [SerializeField] private NetworkRigidbody3D _netRb;
    [SerializeField] private int _damage;
    [SerializeField] private float _lifeTime;
    [SerializeField] private float _movementForce;


    private TickTimer _lifeTimer;

    public override void Spawned()
    {
        _netRb.Rigidbody.AddForce(transform.right * _movementForce, ForceMode.VelocityChange);

        _lifeTimer = TickTimer.CreateFromSeconds(Runner, _lifeTime);    
    }

    public override void FixedUpdateNetwork()
    {
        if(!_lifeTimer.Expired(Runner)) return;

        Runner.Despawn(Object);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out HealthComponent enemyHealthComponent))
        {
            //enemyHealthComponent.TakeDamage();
        }

        Runner.Despawn(Object);
    }

    //Chequear si el tiempo de vida finalizo para despawnear

    //Chequear si colisiono (trigger) con un jugador y sacarle vida

}

using Fusion;
using UnityEngine;

public class HealthComponent : NetworkBehaviour
{
    //Variables requeridas: Vida Maxikma , vida actual
    [SerializeField] private int _maxHealth;
    private int _currentHealth;

    public override void Spawned()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;

        if (_currentHealth <= 0)
        {
            Death();
        }
    }

    public void Death()
    {
        //metodo muerte (siel obj no tiene autoriad input desconcetarlo)
        //ademas de eso despawnearlos
        if (!HasInputAuthority)
        {
            Runner.Disconnect(Object.InputAuthority);
        }

    
        Runner.Despawn(Object);
       
    }
}

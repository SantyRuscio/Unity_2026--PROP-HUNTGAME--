using UnityEngine;
using Fusion;

public class HealthComponent : NetworkBehaviour
{
    [SerializeField] private int _maxHealth = 100;

    [Networked]
    private int CurrentHealth { get; set; }

    [Networked]
    public NetworkBool IsDead { get; set; }

    public override void Spawned()
    {
        if (Runner.IsServer)
        {
            CurrentHealth = _maxHealth;
            IsDead = false;
        }
    }

    public void TakeDamage(int amount)
    {
        if (!Runner.IsServer || IsDead) return;

        CurrentHealth -= amount;

        if (CurrentHealth <= 0)
        {
            IsDead = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndGame(true);
            }
        }
    }

    public void ResetHealth()
    {
        if (Runner.IsServer)
        {
            CurrentHealth = _maxHealth;
            IsDead = false;
        }
    }
}
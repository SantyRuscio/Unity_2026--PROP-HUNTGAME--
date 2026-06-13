using Fusion;
using System;
using UnityEngine;

public class HealthComponent : NetworkBehaviour
{
    [SerializeField] private int _maxHealth;
    [Networked, OnChangedRender(nameof(HealthChanged))] private int CurrentHealth { get; set; }

    [SerializeField] private int _maxLives;
    private int _currentLives;

    [SerializeField] private float _respawnTime;
    [Networked, OnChangedRender(nameof(DeadStateChanged))] private bool IsDead { get; set; }

    event Action<bool> OnDeathUpdate;

    TickTimer _RespawnTimer;

    public override void Spawned()
    {
        CurrentHealth = _maxHealth;
        _currentLives = _maxLives; 
    }

    public override void Render()
    {
        if (!_RespawnTimer.Expired(Runner)) return;

        _RespawnTimer = TickTimer.None;
        CurrentHealth = _maxHealth;
        IsDead = false;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        CurrentHealth -= amount;

        if (CurrentHealth > 0) return;

        if (_currentLives <= 0)
        {
            RPC_NotifyGameOver("¡Juego Terminado! Un jugador ha sido eliminado y el otro gana.");
            DisconnectPlayer();
            return;
        }

        _currentLives--;
        Respawn();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NotifyGameOver(string message)
    {
        Debug.LogWarning("⭐⭐⭐ " + message + " ⭐⭐⭐");
    }

    void Respawn()
    {
        IsDead = true;
        _RespawnTimer = TickTimer.CreateFromSeconds(Runner, _respawnTime);
    }

    void DeadStateChanged()
    {
        OnDeathUpdate?.Invoke(IsDead);
    }

    void HealthChanged()
    {
    }

    public void DisconnectPlayer()
    {
        if (!HasInputAuthority)
        {
            Runner.Disconnect(Object.InputAuthority);
        }
        Runner.Despawn(Object);
    }
}
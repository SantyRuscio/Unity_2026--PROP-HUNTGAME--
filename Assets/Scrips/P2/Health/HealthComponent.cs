using Fusion;
using System;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

public class HealthComponent : NetworkBehaviour
{
    //Variables requeridas: Vida Maxikma , vida actual
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
        CurrentHealth = _maxLives;

    }

    public override void Render()
    {
        if (!_RespawnTimer.Expired(Runner)) return;
        {
            _RespawnTimer = TickTimer.None; 
            CurrentHealth = _maxHealth;

            IsDead = false;
        }
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;

        if (CurrentHealth > 0) return;

        if(_currentLives <= 0)
        {
            DisconnectPlayer();
            return;
        }

        _currentLives--;

    }

    void Respawn()
    {
        IsDead = true;

        _RespawnTimer = TickTimer.CreateFromSeconds(Runner, _respawnTime);
    }

    void DeadStateChanged()
    {
        //prodriamos acceder al hitbox y apagarlo

        OnDeathUpdate?.Invoke(IsDead);
    }


    void HealthChanged()
    {
      //se ejecuta en todos los clientes , por lo que es bueno para actualizar barra vida
    }


    public void DisconnectPlayer()
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

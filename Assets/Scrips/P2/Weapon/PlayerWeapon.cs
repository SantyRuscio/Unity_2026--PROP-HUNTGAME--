using Fusion;
using System;
using UnityEngine;

public class PlayerWeapon : NetworkBehaviour
{

    [SerializeField] private NetworkPrefabRef _bulletPrefab;
    [SerializeField] private Transform _spawnPoint;

    public event Action OnShot;

    public void ShootGameObject()
    {
        if(!HasStateAuthority) return;

        Runner.Spawn(_bulletPrefab, _spawnPoint.position, _spawnPoint.rotation);

        OnShot?.Invoke();
    }

    public void ShootRaycast()
    {
        if(!HasStateAuthority) return;

        //SALE SOLO PARA ADELANE 2D , DEPUES MODIFICAR PARA 3D//
        var raycastBool = Runner.LagCompensation.Raycast(origin: transform.position + Vector3.up, direction: transform.right,
                                                         length: 100, player: Object.InputAuthority, hit: out var HitInfo);


        OnShot?.Invoke();

        if ( !raycastBool ) return;

        if(HitInfo.Hitbox.Root.TryGetComponent(out HealthComponent healthComponent))
        {
            healthComponent.TakeDamage(100);
        }
    }

}

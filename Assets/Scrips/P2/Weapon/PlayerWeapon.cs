using Fusion;
using System;
using UnityEngine;

public class PlayerWeapon : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef _bulletPrefab; // Ya no la usas pero la dejamos por si acaso
    [SerializeField] private Transform _spawnPoint; //

    public event Action OnShot; //

    public void ShootGameObject() //
    {
        if (!HasStateAuthority) return; //
        Runner.Spawn(_bulletPrefab, _spawnPoint.position, _spawnPoint.rotation); //
        OnShot?.Invoke(); //
    }

    public void ShootRaycast()
    {
        // Solo el servidor/StateAuthority procesa la lógica del tiro
        if (!HasStateAuthority) return;

        // 1. El origen nace un poco elevado enfrente del player y viaja hacia adelante (3D)
        Vector3 origin = transform.position + (Vector3.up * 1.2f);
        Vector3 direction = transform.forward;

        // Dibujamos el rayo de debug local en la escena
        Debug.DrawRay(origin, direction * 100f, Color.green, 0.5f);

        // 2. Ejecutamos el Raycast con la compensación de lag de Fusion
        var raycastBool = Runner.LagCompensation.Raycast(
            origin: origin,
            direction: direction,
            length: 100f,
            player: Object.InputAuthority,
            hit: out var HitInfo
        );

        // --- ¡CRUCIAL! ---
        // Al invocar esto en el StateAuthority (Servidor), se cambia la variable en red 'HasShot'
        // lo que disparará el método '[OnChangedRender]' en todos los clientes conectados.
        OnShot?.Invoke();
        // -----------------

        if (!raycastBool) return;

        // 3. Filtro anti-suicidio y daño
        if (HitInfo.Hitbox != null)
        {
            if (HitInfo.Hitbox.Root.Object == this.Object) return;

            if (HitInfo.Hitbox.Root.TryGetComponent(out HealthComponent healthComponent))
            {
                healthComponent.TakeDamage(100);
            }
        }
    }
}
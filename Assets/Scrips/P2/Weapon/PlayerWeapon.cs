using Fusion;
using System;
using UnityEngine;

public class PlayerWeapon : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef _bulletPrefab;
    [SerializeField] private Transform _spawnPoint;

    public event Action OnShot;

    [Header("Shoot Collision Settings")]
    [SerializeField] private LayerMask _blockingLayers;

    public void ShootGameObject()
    {
        if (!HasStateAuthority) return;

        Runner.Spawn(_bulletPrefab, _spawnPoint.position, _spawnPoint.rotation);
        OnShot?.Invoke();
    }

    public void ShootRaycast()
    {
        if (!HasStateAuthority) return;

        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("Main Camera not found.");
            return;
        }

        Vector3 origin = cam.transform.position;
        Vector3 direction = cam.transform.forward;

        float maxDistance = 100f;

        Debug.Log("========== SHOOT ==========");
        Debug.Log($"Origin: {origin}");
        Debug.Log($"Direction: {direction}");

        // Verde = Dirección completa del disparo
        Debug.DrawRay(origin, direction * maxDistance, Color.green, 5f);

        float distanceToObstacle = maxDistance;

        // Raycast contra el escenario
        if (Physics.Raycast(origin, direction, out RaycastHit environmentHit, maxDistance, _blockingLayers))
        {
            if (environmentHit.collider.GetComponentInParent<Hitbox>() == null)
            {
                distanceToObstacle = environmentHit.distance;

                Debug.Log($"[Environment] Hit: {environmentHit.collider.name}");
                Debug.Log($"Distance: {environmentHit.distance:F2}");
                Debug.Log($"Point: {environmentHit.point}");
                Debug.Log($"Normal: {environmentHit.normal}");

                // Rojo = Trayectoria hasta la pared
                Debug.DrawLine(origin, environmentHit.point, Color.red, 5f);

                // Magenta = Normal de la pared
                Debug.DrawRay(environmentHit.point, environmentHit.normal, Color.magenta, 5f);
            }
        }
        else
        {
            Debug.Log("[Environment] No obstacle detected.");
        }

        // Azul = Raycast que realmente usa Lag Compensation
        Debug.DrawRay(origin, direction * distanceToObstacle, Color.blue, 5f);

        bool hitSomething = Runner.LagCompensation.Raycast(
            origin: origin,
            direction: direction,
            length: distanceToObstacle,
            player: Object.InputAuthority,
            hit: out var hitInfo
        );

        OnShot?.Invoke();

        if (!hitSomething)
        {
            Debug.Log("[Lag Compensation] No player hit.");
            return;
        }

        if (hitInfo.Hitbox == null)
        {
            Debug.Log("[Lag Compensation] Raycast hit something, but no Hitbox.");
            return;
        }

        if (hitInfo.Hitbox.Root.Object != null &&
            hitInfo.Hitbox.Root.Object == Object)
        {
            Debug.Log("[Lag Compensation] Ignored self hit.");
            return;
        }

        Debug.Log("========== PLAYER HIT ==========");
        Debug.Log($"Player: {hitInfo.Hitbox.Root.name}");
        Debug.Log($"Hitbox: {hitInfo.Hitbox.name}");
        Debug.Log($"Distance: {hitInfo.Distance:F2}");
        Debug.Log($"Point: {hitInfo.Point}");
        Debug.Log($"Normal: {hitInfo.Normal}");

        // Amarillo = Trayectoria hasta el jugador
        Debug.DrawLine(origin, hitInfo.Point, Color.yellow, 5f);

        // Cian = Normal de la hitbox
        Debug.DrawRay(hitInfo.Point, hitInfo.Normal * 0.5f, Color.cyan, 5f);

        if (hitInfo.Hitbox.Root.TryGetComponent(out HealthComponent health))
        {
            health.TakeDamage(20);
            Debug.Log("[Damage] 20 damage applied.");
        }
        else
        {
            Debug.LogWarning("[Damage] HealthComponent not found.");
        }
    }
}
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

        Vector3 origin = transform.position + (Vector3.up * 1.2f);

        float pitch = 0f;
        if (TryGetComponent(out PlayerController controller))
        {
            pitch = controller.CameraPitch;
        }

        Quaternion aimRotation = transform.rotation * Quaternion.Euler(pitch, 0, 0);
        Vector3 direction = aimRotation * Vector3.forward;

        float maxDistance = 100f;

        Debug.DrawRay(origin, direction * maxDistance, Color.green, 0.5f);

        float distanceToObstacle = maxDistance;
        if (Physics.Raycast(origin, direction, out RaycastHit environmentHit, maxDistance, _blockingLayers))
        {
            if (environmentHit.collider.GetComponentInParent<Hitbox>() == null)
            {
                distanceToObstacle = environmentHit.distance;
                Debug.Log($"[Environment Detected] Wall hit at {distanceToObstacle} meters.");
            }
        }

        var raycastBool = Runner.LagCompensation.Raycast(
            origin: origin,
            direction: direction,
            length: distanceToObstacle,
            player: Object.InputAuthority,
            hit: out var HitInfo
        );

        OnShot?.Invoke();

        if (!raycastBool) return;

        if (HitInfo.Hitbox != null)
        {
            if (HitInfo.Hitbox.Root.Object != null && HitInfo.Hitbox.Root.Object == Object)
                return;

            Debug.Log($"Hit confirmed on player: {HitInfo.Hitbox.Root.name}");

            if (HitInfo.Hitbox.Root.TryGetComponent(out HealthComponent health))
            {
                health.TakeDamage(20);
            }
        }
    }
}
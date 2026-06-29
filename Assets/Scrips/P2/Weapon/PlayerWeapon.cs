using Fusion;
using System;
using UnityEngine;

public class PlayerWeapon : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef _bulletPrefab;
    [SerializeField] private Transform _spawnPoint;

    public event Action OnShot;

    [Header("Configuración de Colisión de Tiro")]
    [SerializeField] private LayerMask _capasQueBloqueanTiro;
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

        float distanciaAObstaculo = maxDistance;
        if (Physics.Raycast(origin, direction, out RaycastHit hitEscenario, maxDistance, _capasQueBloqueanTiro))
        {
            if (hitEscenario.collider.GetComponentInParent<Hitbox>() == null)
            {
                distanciaAObstaculo = hitEscenario.distance;
                Debug.Log($"[Escenario Detectado] Hay una pared a {distanciaAObstaculo} metros.");
            }
        }

        var raycastBool = Runner.LagCompensation.Raycast(
            origin: origin,
            direction: direction,
            length: distanciaAObstaculo, 
            player: Object.InputAuthority,
            hit: out var HitInfo
        );

        OnShot?.Invoke();

        if (!raycastBool) return;

        if (HitInfo.Hitbox != null)
        {
            if (HitInfo.Hitbox.Root.Object != null && HitInfo.Hitbox.Root.Object == Object)
                return; 

            Debug.Log($"¡Impacto confirmado en jugador!: {HitInfo.Hitbox.Root.name}");

            if (HitInfo.Hitbox.Root.TryGetComponent(out HealthComponent health))
            {
                health.TakeDamage(20);
            }
        }
    }
}
using Fusion;
using System;
using UnityEngine;

public class PlayerWeapon : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef _bulletPrefab;
    [SerializeField] private Transform _spawnPoint;

    public event Action OnShot;

    [Header("Configuración de Colisión de Tiro")]
    [SerializeField] private LayerMask _capasQueBloqueanTiro; // Configurá acá "Default", "Paredes", etc.

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
        Vector3 direction = transform.forward;
        float maxDistance = 100f;

        // Dibujamos el rayo de debug en la escena
        Debug.DrawRay(origin, direction * maxDistance, Color.green, 0.5f);

        float distanciaAObstaculo = maxDistance;
        if (Physics.Raycast(origin, direction, out RaycastHit hitEscenario, maxDistance, _capasQueBloqueanTiro))
        {
            // Si el objeto que tocamos NO tiene una Hitbox de jugador, es una pared del escenario
            if (hitEscenario.collider.GetComponentInParent<Hitbox>() == null)
            {
                distanciaAObstaculo = hitEscenario.distance;
                Debug.Log($"[Escenario Detectado] Hay una pared a {distanciaAObstaculo} metros.");
            }
        }

        // 2. Ejecutamos el Raycast de Fusion para registrar el impacto sobre los jugadores
        var raycastBool = Runner.LagCompensation.Raycast(
            origin: origin,
            direction: direction,
            length: distanciaAObstaculo, // Le pasamos el límite de la pared para que no la atraviese
            player: Object.InputAuthority,
            hit: out var HitInfo
        );

        // Disparamos el evento visual (partículas, sonido, etc.) para los clientes
        OnShot?.Invoke();

        if (!raycastBool) return;

        // 3. Si impactó en la Hitbox de otro jugador dentro del rango permitido (antes de la pared)
        if (HitInfo.Hitbox != null)
        {
            if (HitInfo.Hitbox.Root.Object != null && HitInfo.Hitbox.Root.Object == Object)
                return; // Anti-suicidio

            Debug.Log($"¡Impacto confirmado en jugador!: {HitInfo.Hitbox.Root.name}");

            if (HitInfo.Hitbox.Root.TryGetComponent(out HealthComponent health))
            {
                health.TakeDamage(20); // Aplica el daño correspondiente
            }
        }
    }
}
using UnityEngine;
using Fusion;

public class PlayerCamera : NetworkBehaviour
{
    [Header("Configuración Base")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -4);
    [SerializeField] private float smoothSpeed = 10f;

    [Header("Colisión de Cámara")]
    [SerializeField] private LayerMask collisionLayers; 
    [SerializeField] private float cameraRadius = 0.2f; 
    [SerializeField] private float minDistance = 0.5f;  

    private Camera _cam;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            _cam = Camera.main;

            if (collisionLayers == 0)
            {
                collisionLayers = ~LayerMask.GetMask("Ignore Raycast");
            }
        }
        else
        {
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (_cam == null) return;

        float currentPitch = 0f;
        if (TryGetComponent(out PlayerController controller))
        {
            currentPitch = controller.CameraPitch;
        }

        Quaternion rotacionTotal = transform.rotation * Quaternion.Euler(currentPitch, 0, 0);
        Vector3 targetPos = transform.position + rotacionTotal * offset;

        Vector3 originPos = transform.position + Vector3.up * offset.y;

        Vector3 finalPos = targetPos;
        Vector3 rayDirection = targetPos - originPos;
        float maxDistance = rayDirection.magnitude;

        if (Physics.SphereCast(originPos, cameraRadius, rayDirection.normalized, out RaycastHit hit, maxDistance, collisionLayers))
        {
            float clampedDistance = Mathf.Max(hit.distance, minDistance);
            finalPos = originPos + rayDirection.normalized * clampedDistance;
        }

        _cam.transform.position = Vector3.Lerp(_cam.transform.position, finalPos, smoothSpeed * Time.deltaTime);
        _cam.transform.LookAt(transform.position + Vector3.up * 1.5f);
    }
}
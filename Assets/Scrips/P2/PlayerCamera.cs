using UnityEngine;
using Fusion;

public class PlayerCamera : NetworkBehaviour
{
    [Header("General Settings")]
    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private float _smoothSpeed = 20f;

    [Header("Third Person Settings (Propper)")]
    [SerializeField] private float _tpDistance = 4f;
    [SerializeField] private Vector3 _tpOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private LayerMask _collisionLayers;

    private Camera _cam;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            _cam = Camera.main;
        }
        else
        {
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (_cam == null || _cameraPivot == null) return;

        if (TryGetComponent(out PlayerController controller))
        {
            float currentPitch = controller.CameraPitch;
            float currentYaw = controller.CameraYaw; 
            bool isHunter = controller.IsHunter;

            Quaternion targetRotation = Quaternion.Euler(currentPitch, currentYaw, 0);
            Vector3 targetPosition;

            if (isHunter)
            {
                targetPosition = _cameraPivot.position;
            }
            else
            {
                Vector3 startPos = _cameraPivot.position + _tpOffset;
                Vector3 direction = targetRotation * Vector3.back;
                Vector3 desiredPos = startPos + (direction * _tpDistance);

                if (Physics.SphereCast(startPos, 0.2f, direction, out RaycastHit hit, _tpDistance, _collisionLayers))
                {
                    targetPosition = hit.point + (hit.normal * 0.1f);
                }
                else
                {
                    targetPosition = desiredPos;
                }
            }

            _cam.transform.position = Vector3.Lerp(_cam.transform.position, targetPosition, _smoothSpeed * Time.deltaTime);
            _cam.transform.rotation = targetRotation;
        }
    }
}
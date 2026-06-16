using UnityEngine;
using Fusion;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -4);
    [SerializeField] private float smoothSpeed = 10f;

    private Camera _cam;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            _cam = Camera.main;
        }
        else
        {
            enabled = false; // solo corre para tu player local
        }
    }

    private void LateUpdate()
    {
        if (_cam == null) return;

        Vector3 targetPos = transform.position + transform.rotation * offset;
        _cam.transform.position = Vector3.Lerp(_cam.transform.position, targetPos, smoothSpeed * Time.deltaTime);
        _cam.transform.LookAt(transform.position + Vector3.up * 1.5f);
    }
}
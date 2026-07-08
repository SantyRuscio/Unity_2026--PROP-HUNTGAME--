using UnityEngine;
using Fusion;

public class PlayerCamera : NetworkBehaviour
{
    [Header("Configuración de Cámara en 1ra Persona")]
    // Aquí arrastras el objeto vacío desde el inspector
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float smoothSpeed = 20f;

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
        if (_cam == null || cameraTarget == null) return;

        // Obtenemos el Pitch del controlador para inclinar la cámara
        float currentPitch = 0f;
        if (TryGetComponent(out PlayerController controller))
        {
            currentPitch = controller.CameraPitch;
        }

        // La posición es la del objeto que arrastraste al inspector
        Vector3 targetPos = cameraTarget.position;

        // La rotación es la del jugador (giro horizontal) + el Pitch (inclinación vertical)
        Quaternion rotacionTotal = transform.rotation * Quaternion.Euler(currentPitch, 0, 0);

        // Suavizado de posición
        _cam.transform.position = Vector3.Lerp(_cam.transform.position, targetPos, smoothSpeed * Time.deltaTime);

        // Aplicamos la rotación
        _cam.transform.rotation = rotacionTotal;
    }
}
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform _target;

    [Header("Distance")]
    [SerializeField] private float _distance = -7f; 
    [SerializeField] private float _height = 2.5f;

    [Header("Mouse")]
    [SerializeField] private float _sensX = 200f;
    [SerializeField] private float _sensY = 150f;

    [Header("Colisiones (Anti-Paredes)")]
    [SerializeField] private LayerMask _wallLayer; 
    [SerializeField] private float _cameraRadius = 0.3f; 

    private float _yaw;
    private float _pitch = 20f;

    public void SetTarget(Transform target)
    {
        _target = target;

        if (_target != null)
            _yaw = _target.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        float mouseX = Input.GetAxis("Mouse X") * _sensX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _sensY * Time.deltaTime;

        _yaw += mouseX;
        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, -30f, 70f);

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);


        Vector3 direction = rotation * new Vector3(0, 0, _distance);
        Vector3 pivotPosition = _target.position + Vector3.up * _height; 
        Vector3 desiredPosition = pivotPosition + direction;


        Vector3 castDirection = (desiredPosition - pivotPosition).normalized;
        float castDistance = Vector3.Distance(pivotPosition, desiredPosition);


        if (Physics.SphereCast(pivotPosition, _cameraRadius, castDirection, out RaycastHit hit, castDistance, _wallLayer))
        {
            transform.position = hit.point + hit.normal * 0.1f;
        }
        else
        {
            transform.position = desiredPosition;
        }

        transform.LookAt(_target.position + Vector3.up * 1.5f);
    }

    public float GetYaw()
    {
        return _yaw;
    }
}
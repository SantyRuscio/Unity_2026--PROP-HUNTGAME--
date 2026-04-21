using UnityEngine;


public class CameraFollow : MonoBehaviour
{
    private Transform _target;

    [SerializeField] private Vector3 _offset = new Vector3(0, 5.5f, -7);
    [SerializeField] private float _smoothSpeed = 10f;

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 desiredPosition = _target.position + _offset;

        Vector3 smoothPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            _smoothSpeed * Time.deltaTime
        );

        transform.position = smoothPosition;

        transform.LookAt(_target);
    }
}

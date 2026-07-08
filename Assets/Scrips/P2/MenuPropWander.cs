using UnityEngine;

public class MenuPropWanderer : MonoBehaviour
{
    [Header("Wander Settings")]
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _wanderRadius = 4f;

    [Header("Wait Times")]
    [SerializeField] private float _minWaitTime = 1f;
    [SerializeField] private float _maxWaitTime = 4f;

    [Header("Collision Avoidance")]
    [SerializeField] private LayerMask _obstacleLayers; 
    [SerializeField] private float _sensorLength = 1f;  

    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private bool _isMoving = false;
    private float _waitTimer = 0f;

    private void Start()
    {
        _startPosition = transform.position;
        PickNewDestination();
    }

    private void Update()
    {
        if (_isMoving)
        {
            MoveTowardsTarget();
        }
        else
        {
            WaitAndPickNewDestination();
        }
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (_targetPosition - transform.position).normalized;

        Vector3 rayStart = transform.position + Vector3.up * 0.2f;
        if (Physics.Raycast(rayStart, direction, _sensorLength, _obstacleLayers))
        {
            _isMoving = false;
            _waitTimer = 0.2f;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _moveSpeed * Time.deltaTime);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, _targetPosition) < 0.1f)
        {
            _isMoving = false;
            _waitTimer = Random.Range(_minWaitTime, _maxWaitTime);
        }
    }

    private void WaitAndPickNewDestination()
    {
        _waitTimer -= Time.deltaTime;

        if (_waitTimer <= 0f)
        {
            PickNewDestination();
        }
    }

    private void PickNewDestination()
    {
        Vector2 randomPoint = Random.insideUnitCircle * _wanderRadius;
        _targetPosition = _startPosition + new Vector3(randomPoint.x, 0f, randomPoint.y);
        _isMoving = true;
    }
}
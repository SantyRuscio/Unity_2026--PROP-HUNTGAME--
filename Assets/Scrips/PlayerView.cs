using Fusion;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [Header("Modelos Visuales")]
    [SerializeField] private GameObject _humanMesh;
    [SerializeField] private GameObject[] _propMeshes;

    private int _lastVisualID = 0;
    private PlayerController _playerController;

    private void OnEnable()
    {
        _playerController = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        if (_playerController == null) return;

        if (_playerController.CurrentPropID != _lastVisualID)
        {
            SwapModel(_playerController.CurrentPropID);
            _lastVisualID = _playerController.CurrentPropID;
        }
    }

    void SwapModel(int id)
    {
        if (id == 0)
        {
            _humanMesh.SetActive(true);
            foreach (var mesh in _propMeshes) mesh.SetActive(false);
        }
        else
        {
            _humanMesh.SetActive(false);
            for (int i = 0; i < _propMeshes.Length; i++)
            {
                _propMeshes[i].SetActive(i == (id - 1));
            }
        }
    }
}
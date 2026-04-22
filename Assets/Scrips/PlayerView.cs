using Fusion;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField] private NetworkMecanimAnimator _mecanim;
    [SerializeField] private ParticleSystem _fireParticles;

    [Header("Mecánica Extra")]
    [SerializeField] private AudioSource _tauntAudioSource;

    [Header("Modelos Visuales")]
    [SerializeField] private GameObject _humanMesh; 
    [SerializeField] private GameObject[] _propMeshes;

    private int _lastVisualID = 0;
    private Player _player;

    private void Update()
    {
        if (_player == null) return;

        if (_player.CurrentPropID != _lastVisualID)
        {
            SwapModel(_player.CurrentPropID);
            _lastVisualID = _player.CurrentPropID;
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

    private void OnEnable()
    {
        _player = GetComponentInParent<Player>();

        if (_player != null)
        {
            _player.OnMovement += HandleMovement;
            _player.OnJump += HandleJump;
            _player.OnGroundedChanged += HandleGrounded;

            _player.OnTaunt += HandleTaunt;
        }
    }

    private void OnDisable()
    {
        if (_player != null)
        {
            _player.OnMovement -= HandleMovement;
            _player.OnJump -= HandleJump;
            _player.OnGroundedChanged -= HandleGrounded;

            _player.OnTaunt -= HandleTaunt;
        }
    }

    void HandleMovement(float speed)
    {
        _mecanim.Animator.SetFloat("speed", speed);
    }

    void HandleJump()
    {
        _mecanim.Animator.SetTrigger("jump");
    }

    void HandleGrounded(bool grounded)
    {
        _mecanim.Animator.SetBool("isGrounded", grounded);
    }

    void ShotParticles()
    {
        _fireParticles.Play();
    }

    void HandleTaunt()
    {
        if (_tauntAudioSource != null)
        {
            _tauntAudioSource.Play();
        }
    }
}
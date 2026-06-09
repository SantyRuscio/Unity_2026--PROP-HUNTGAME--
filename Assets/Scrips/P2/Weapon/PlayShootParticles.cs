using Fusion;
using UnityEngine;

public class PlayShotParticles : NetworkBehaviour
{
    [SerializeField] ParticleSystem _particles;

    [SerializeField] PlayerWeapon _weapon;

    [Networked, OnChangedRender(nameof(ShowParticles))]
    private NetworkBool HasShot { get; set; }

    private void Awake()
    {
        _weapon.OnShot += () => HasShot = !HasShot;
    }

    void ShowParticles()
    {
        _particles.Play();
    }
}

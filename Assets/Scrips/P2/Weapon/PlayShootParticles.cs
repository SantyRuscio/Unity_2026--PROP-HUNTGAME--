using Fusion;
using UnityEngine;


//public class PlayShotParticles : NetworkBehaviour
//{
//    [SerializeField] ParticleSystem _particles;
//    [SerializeField] PlayerWeapon _weapon;
//
//    [Networked, OnChangedRender(nameof(ShowParticles))]
//    private NetworkBool HasShot { get; set; }
//
//    private void Awake()
//    {
//        // --- ARREGLO AQUÍ ---
//        // Si te olvidaste de asignarlo en el Inspector, lo busca automáticamente en el objeto
//        if (_weapon == null)
//        {
//            _weapon = GetComponent<PlayerWeapon>();
//        }
//        // ---------------------
//
//        // Ahora estamos 100% seguros de que _weapon no va a ser null
//        if (_weapon != null)
//        {
//            _weapon.OnShot += () => HasShot = !HasShot;
//        }
//        else
//        {
//            Debug.LogError($"¡No se encontró el componente PlayerWeapon en {gameObject.name}!", this);
//        }
//    }
//
//    void ShowParticles()
//    {
//        _particles.Play();
//    }
//}

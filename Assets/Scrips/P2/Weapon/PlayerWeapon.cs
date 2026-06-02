using Fusion;
using UnityEngine;

public class PlayerWeapon : NetworkBehaviour
{
    //Variables Requerdiad ; prefab bala , deonde va a salir la bala
    [SerializeField] private NetworkPrefabRef _bulletPrefab;
    [SerializeField] private Transform _spawnPoint;

    //metodo disparo(llamarlo desde player controlles)

    // importante : solo crear la bala si tenes autoridad de estado

    public void ShootGameObject()
    {
        if(!HasStateAuthority) return;

        Runner.Spawn(_bulletPrefab, _spawnPoint.position, _spawnPoint.rotation);
    }

    public void ShootRaycast()
    {

    }

}

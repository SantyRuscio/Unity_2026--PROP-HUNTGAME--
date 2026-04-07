using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] NetworkPrefabRef _playerprefab;
    public void PlayerJoined(PlayerRef player)
    {
        if(player == Runner.LocalPlayer)
        {
            //creo el pj
            Runner.Spawn(_playerprefab, Vector3.zero, Quaternion.identity);
        }
    }
}
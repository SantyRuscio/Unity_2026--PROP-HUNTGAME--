using Fusion;
using UnityEngine;
using System.Collections.Generic;
public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private NetworkPrefabRef hunterPrefab;
    [SerializeField] private NetworkPrefabRef propPrefab;

    [SerializeField] private Transform hunterSpawn;
    [SerializeField] private Transform propSpawn;

    private List<PlayerRef> players = new List<PlayerRef>();

    public void PlayerJoined(PlayerRef player)
    {
        Debug.Log("Jugador entró");

        if (!Runner.IsServer) return;

        players.Add(player);

        bool isHunter = players.Count == 1;

        if (isHunter)
        {
            Runner.Spawn(hunterPrefab, hunterSpawn.position, hunterSpawn.rotation, player);
        }
        else
        {
            Runner.Spawn(propPrefab, propSpawn.position, propSpawn.rotation, player);
        }
    }
}
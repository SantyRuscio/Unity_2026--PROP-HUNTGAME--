using Fusion;
using UnityEngine;
using System.Collections.Generic;
//public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
//{
//    [SerializeField] private NetworkPrefabRef hunterPrefab;
//    [SerializeField] private NetworkPrefabRef propPrefab;
//
//    [SerializeField] private Transform hunterSpawn;
//    [SerializeField] private Transform propSpawn;
//
//    private List<PlayerRef> players = new List<PlayerRef>();
//
//    public void PlayerJoined(PlayerRef player)
//    {
//        Debug.Log("Jugador entr�");
//
//        if (!Runner.IsServer) return;
//
//        players.Add(player);
//
//        bool isHunter = players.Count == 1;
//
//        if (isHunter)
//        {
//            Runner.Spawn(hunterPrefab, hunterSpawn.position, hunterSpawn.rotation, player);
//        }
//        else
//        {
//            Runner.Spawn(propPrefab, propSpawn.position, propSpawn.rotation, player);
//        }
//    }
//}

//public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
//{
//    Vector3 Pos = new Vector3(1, 1.5f, 1);

//    [SerializeField] private Transform[] _spawnPoints;

//    [SerializeField] private NetworkPrefabRef _playerprefab;
//    public void PlayerJoined(PlayerRef player)
//    {
//        var playersCount = Runner.SessionInfo.PlayerCount;

//        if (playersCount > 2) { return; }

//        //si el cliente que entro es el mismo cliente en donde corre est codigo:
//        if (player == Runner.LocalPlayer)
//        {
//            var slot = playersCount - 1;

//            var spawnPoint = _spawnPoints[slot];

//            Runner.Spawn(_playerprefab, spawnPoint.position, spawnPoint.rotation);
            
//        }
//    }
//}
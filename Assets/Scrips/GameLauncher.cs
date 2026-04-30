using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameLauncher : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;

    public static bool InvertRoles = false;

    [Header("Prefabs Asimétricos")]
    [SerializeField] private NetworkPrefabRef prefabPlayer1;
    [SerializeField] private NetworkPrefabRef prefabPlayer2;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints; 

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    async void Start()
    {
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        _runner.AddCallbacks(this);

        var appSettings = new Fusion.Photon.Realtime.FusionAppSettings();
        appSettings.FixedRegion = "sa";
        appSettings.AppIdFusion = "7fa4c827-8dd3-4a32-9772-161f89dc399d";

        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "SalaObscure",
            CustomPhotonAppSettings = appSettings,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex)
        });
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            NetworkPrefabRef prefabParaSpawnear;

            bool isHost = runner.IsSharedModeMasterClient;

            if (InvertRoles)
            {
                isHost = !isHost;
            }

            if (isHost)
            {
                prefabParaSpawnear = prefabPlayer1;
                Debug.Log("Spawneando como Player 1");
            }
            else
            {
                prefabParaSpawnear = prefabPlayer2;
                Debug.Log("Spawneando como Player 2");
            }
            int spawnIndex = player.RawEncoded % spawnPoints.Length;
            Transform spawn = spawnPoints[spawnIndex];

            runner.Spawn(prefabParaSpawnear, spawn.position, spawn.rotation, player);
        }
    }


    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}
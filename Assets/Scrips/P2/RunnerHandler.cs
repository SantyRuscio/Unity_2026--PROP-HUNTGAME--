using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;

public class RunnerHandler : MonoBehaviour, INetworkRunnerCallbacks
{

    [SerializeField] private NetworkRunner _runnerPrefab;
    private NetworkRunner _currentRunner;

    public event Action OnJoinLobbySuccesfully;

    public event Action<List<SessionInfo>> OnSessionListUpdate;

    private void Awake()
    {
        Debug.Log(SceneUtility.GetBuildIndexByScenePath($"Scenes/Parcial2/Gameplay"));
    }

    public void JoinLobby()
    {
        if (_currentRunner)
        {
            Destroy(_currentRunner.gameObject);
        }

        _currentRunner = Instantiate(_runnerPrefab);
        _currentRunner.AddCallbacks(this);

        JoinLobbyAsync();
    }

    async void JoinLobbyAsync()
    {
        var result = await _currentRunner.JoinSessionLobby(SessionLobby.Custom, "CustomLobby");

        if(result.Ok)
        {
            Debug.Log("Connected To Lobby");
            OnJoinLobbySuccesfully?.Invoke();
        }
        else
        {
            Debug.Log("Fail Join Lobby");
        }
    }

    //CLIENTE
    public void JoinGame(SessionInfo session)
    {
        CreateGame(GameMode.Client, session.Name);
    }


    //host
    public void HostGame(string sessionName, string sceneName)
    {
        CreateGame(GameMode.Host, sessionName, 1);
    }

    async void CreateGame(GameMode gameMode, string sessionName, int sceneIndex = 0)
    {
        _currentRunner.ProvideInput = true;

        var result = await _currentRunner.StartGame(new StartGameArgs()
        {
            GameMode = gameMode,
            SessionName = sessionName,
            Scene = SceneRef.FromIndex(sceneIndex),
            PlayerCount = 2,
            SceneManager = _currentRunner.gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
        {
            Debug.Log("¡Juego iniciado y escena cargada!");
        }
        else
        {
            Debug.Log("Fallo al iniciar el juego");
        }
    }


    private bool _connectingToSession = false;
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        //  if (_connectingToSession) return;
        //  if(sessionList.Count > 0) // SI HAY SESSION CREADA , CONECTARNOS
        //  {
        //      JoinGame(sessionList[0]);
        //  }
        //  else  //SI NO HOSTERAR SALA
        //  {
        //      HostGame("Bla", "GamePlay");
        //  }
        //
        //  _connectingToSession = true;

        OnSessionListUpdate?.Invoke(sessionList);
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {}

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {}

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {}

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {}

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {}

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {}

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {}

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {}

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {}

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {}

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {}

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {}

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {}

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {}

    public void OnSceneLoadDone(NetworkRunner runner)
    {}

    public void OnSceneLoadStart(NetworkRunner runner)
    {}

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {}

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {}
}

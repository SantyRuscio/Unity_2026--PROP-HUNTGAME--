using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class RunnerHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner _runnerPrefab;
    private NetworkRunner _currentRunner;

    public event Action OnJoinLobbySuccesfully;
    public event Action<List<SessionInfo>> OnSessionListUpdate;
    public event Action<int> OnPlayerCountChanged;
    public event Action<bool, bool> OnReadyStateChanged;

    private bool _isLocalReady = false;
    private bool _isRemoteReady = false;

    public void JoinLobby()
    {
        if (_currentRunner) Destroy(_currentRunner.gameObject);
        _currentRunner = Instantiate(_runnerPrefab);
        _currentRunner.AddCallbacks(this);
        JoinLobbyAsync();
    }

    async void JoinLobbyAsync()
    {
        var result = await _currentRunner.JoinSessionLobby(SessionLobby.Custom, "CustomLobby");
        if (result.Ok) OnJoinLobbySuccesfully?.Invoke();
    }

    public void JoinGame(SessionInfo session) { CreateGame(GameMode.Client, session.Name); }
    public void HostGame(string sessionName, string sceneName) { CreateGame(GameMode.Host, sessionName); }

    async void CreateGame(GameMode gameMode, string sessionName)
    {
        _currentRunner.ProvideInput = true;

        var result = await _currentRunner.StartGame(new StartGameArgs()
        {
            GameMode = gameMode,
            SessionName = sessionName,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            PlayerCount = 2,
            SceneManager = _currentRunner.gameObject.GetComponent<NetworkSceneManagerDefault>() ?? _currentRunner.gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
        {
            _isLocalReady = false;
            _isRemoteReady = false;
            OnPlayerCountChanged?.Invoke(_currentRunner.SessionInfo.PlayerCount);
            OnReadyStateChanged?.Invoke(false, false);
        }
    }

    public void TogglePlayerReady()
    {
        if (_currentRunner == null) return;

        _isLocalReady = !_isLocalReady;

        if (_currentRunner.IsServer)
        {
            byte[] data = new byte[] { 2, (byte)(_isLocalReady ? 1 : 0) };
            foreach (var player in _currentRunner.ActivePlayers)
            {
                if (player != _currentRunner.LocalPlayer)
                    _currentRunner.SendReliableDataToPlayer(player, default, data);
            }
            OnReadyStateChanged?.Invoke(_isLocalReady, _isRemoteReady);
            CheckGameStart();
        }
        else
        {
            byte[] data = new byte[] { 1, (byte)(_isLocalReady ? 1 : 0) };
            _currentRunner.SendReliableDataToPlayer(PlayerRef.None, default, data);
            OnReadyStateChanged?.Invoke(_isRemoteReady, _isLocalReady);
        }
    }

    private void CheckGameStart()
    {
        if (_currentRunner.IsServer)
        {
            if (_isLocalReady && _isRemoteReady && _currentRunner.SessionInfo.PlayerCount >= 2)
            {
                _currentRunner.LoadScene(SceneRef.FromIndex(1));
            }
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { OnPlayerCountChanged?.Invoke(runner.SessionInfo.PlayerCount); }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        OnPlayerCountChanged?.Invoke(runner.SessionInfo.PlayerCount);
        _isRemoteReady = false;
        OnReadyStateChanged?.Invoke(_isLocalReady, false);
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        if (data.Count < 2) return;

        byte role = data.Array[data.Offset];
        byte readyState = data.Array[data.Offset + 1];
        bool isReady = (readyState == 1);

        if (runner.IsServer && role == 1)
        {
            _isRemoteReady = isReady;
            OnReadyStateChanged?.Invoke(_isLocalReady, _isRemoteReady);
            CheckGameStart();
        }
        else if (!runner.IsServer && role == 2)
        {
            _isRemoteReady = isReady;
            OnReadyStateChanged?.Invoke(_isRemoteReady, _isLocalReady);
        }
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { OnSessionListUpdate?.Invoke(sessionList); }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}
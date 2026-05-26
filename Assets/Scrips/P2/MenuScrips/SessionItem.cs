using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _sessionName;
    [SerializeField] private TMP_Text _playerCount;
    [SerializeField] private Button _joinButton;

    [SerializeField] RunnerHandler _runnerHandler;
    private SessionInfo _sessionInfo;
    public void Initialize(SessionInfo sessionInfo, RunnerHandler runnerHandler)
    {
        _runnerHandler = runnerHandler;
        _sessionInfo = sessionInfo;

        _sessionName.text = sessionInfo.Name;

        _playerCount.text = $"{sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers}";

        _joinButton.interactable = sessionInfo.PlayerCount < sessionInfo.MaxPlayers;

        _joinButton.onClick.AddListener(CallToJoinGame);
    }

    void CallToJoinGame()
    {
        _joinButton.interactable = false;

        _runnerHandler.JoinGame(_sessionInfo);
    }
}

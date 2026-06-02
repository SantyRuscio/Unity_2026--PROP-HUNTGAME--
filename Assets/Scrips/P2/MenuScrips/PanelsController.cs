using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelsController : MonoBehaviour
{
    [SerializeField] RunnerHandler _runnerHandler;

    [Header("Panels")]
    [SerializeField] private GameObject _initialPanel;
    [SerializeField] private GameObject _joiningPanel;
    [SerializeField] private GameObject _sessionBrowserPanel;
    [SerializeField] private GameObject _hostPanel;

    [Header("Buttons")]
    [SerializeField] private Button _joinLobbyButton;
    [SerializeField] private Button _goToHostPanelButton;
    [SerializeField] private Button _hostButton;

    [Header("InputFields")]
    [SerializeField] private TMP_InputField _inputSessionName;


    private void Awake()
    {
        _joinLobbyButton.onClick.AddListener(AskToJoinLobby);

        _runnerHandler.OnJoinLobbySuccesfully += () =>
        {
            _joiningPanel.SetActive(false);
            _sessionBrowserPanel.SetActive(true);
        };

        _goToHostPanelButton.onClick.AddListener(() =>
        {
            _sessionBrowserPanel.SetActive(false);
            _hostPanel.SetActive(true); 
        });

        _hostButton.onClick.AddListener(StartGameAsHost);
    }

    void AskToJoinLobby()
    {
        _joinLobbyButton.interactable = false;

        _runnerHandler.JoinLobby();

        _initialPanel.SetActive(false);
        _joiningPanel.SetActive(true);
    }

    private void StartGameAsHost()
    {
        _hostButton.interactable=false;

        _runnerHandler.HostGame(_inputSessionName.text ,"Gameplay");
    }

}

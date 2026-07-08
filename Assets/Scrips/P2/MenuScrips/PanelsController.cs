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
    [SerializeField] private GameObject _howToPlayPanel;

    [Header("Lobby UI Elements")]
    [SerializeField] private GameObject _menuLobbyPanel;
    [SerializeField] private TMP_Text _lobbyStatusText;
    [SerializeField] private Button _lobbyReadyButton;

    // 🔥 LOS NUEVOS CONTENEDORES Y TEXTOS
    [SerializeField] private GameObject _p1Container;
    [SerializeField] private TMP_Text _p1ReadyText;
    [SerializeField] private GameObject _p2Container;
    [SerializeField] private TMP_Text _p2ReadyText;

    [Header("Buttons")]
    [SerializeField] private Button _joinLobbyButton;
    [SerializeField] private Button _goToHostPanelButton;
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _howToPlayButton;
    [SerializeField] private Button _closeHowToPlayButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _backFromBrowserButton;
    [SerializeField] private Button _backFromHostButton;

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

        if (_howToPlayButton != null) _howToPlayButton.onClick.AddListener(() => { _initialPanel.SetActive(false); _howToPlayPanel.SetActive(true); });
        if (_closeHowToPlayButton != null) _closeHowToPlayButton.onClick.AddListener(() => { _howToPlayPanel.SetActive(false); _initialPanel.SetActive(true); });
        if (_exitButton != null) _exitButton.onClick.AddListener(ExitGame);
        if (_backFromBrowserButton != null) _backFromBrowserButton.onClick.AddListener(() => { _sessionBrowserPanel.SetActive(false); _initialPanel.SetActive(true); _joinLobbyButton.interactable = true; });
        if (_backFromHostButton != null) _backFromHostButton.onClick.AddListener(() => { _hostPanel.SetActive(false); _sessionBrowserPanel.SetActive(true); });

        // Eventos del Lobby Visual
        _runnerHandler.OnPlayerCountChanged += UpdateLobbyUI;
        _runnerHandler.OnReadyStateChanged += UpdateReadyVisuals; // Conectamos los textos

        if (_lobbyReadyButton != null)
        {
            _lobbyReadyButton.onClick.AddListener(() =>
            {
                _lobbyReadyButton.interactable = false;
                _runnerHandler.SetPlayerReady();
            });
        }
    }

    private void UpdateLobbyUI(int playerCount)
    {
        _sessionBrowserPanel.SetActive(false);
        _hostPanel.SetActive(false);
        _menuLobbyPanel.SetActive(true);

        if (_lobbyStatusText != null && _lobbyReadyButton.interactable)
        {
            _lobbyStatusText.text = $"Players Connected: {playerCount}/2";
        }

        // Si el segundo jugador no está, apagamos su imagen y ponemos su texto gris
        if (_p1Container != null) _p1Container.SetActive(true);
        if (_p2Container != null) _p2Container.SetActive(playerCount >= 2);

        if (playerCount < 2 && _p2ReadyText != null)
        {
            _p2ReadyText.text = "WAITING...";
            _p2ReadyText.color = Color.gray;
        }
    }

    private void UpdateReadyVisuals(bool hostReady, bool clientReady)
    {
        if (_p1ReadyText != null)
        {
            _p1ReadyText.text = hostReady ? "READY" : "NOT READY";
            _p1ReadyText.color = hostReady ? Color.green : Color.red;
        }

        if (_p2ReadyText != null)
        {
            _p2ReadyText.text = clientReady ? "READY" : "NOT READY";
            _p2ReadyText.color = clientReady ? Color.green : Color.red;
        }
    }

    void AskToJoinLobby() { _joinLobbyButton.interactable = false; _runnerHandler.JoinLobby(); _initialPanel.SetActive(false); _joiningPanel.SetActive(true); }
    private void StartGameAsHost() { _hostButton.interactable = false; _runnerHandler.HostGame(_inputSessionName.text, "Gameplay"); }
    private void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
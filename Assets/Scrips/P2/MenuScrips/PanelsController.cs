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

        if (_howToPlayButton != null)
        {
            _howToPlayButton.onClick.AddListener(() =>
            {
                _initialPanel.SetActive(false);
                _howToPlayPanel.SetActive(true);
            });
        }

        if (_closeHowToPlayButton != null)
        {
            _closeHowToPlayButton.onClick.AddListener(() =>
            {
                _howToPlayPanel.SetActive(false);
                _initialPanel.SetActive(true);
            });
        }

        if (_exitButton != null)
        {
            _exitButton.onClick.AddListener(SalirDelJuego);
        }

        if (_backFromBrowserButton != null)
        {
            _backFromBrowserButton.onClick.AddListener(() =>
            {
                _sessionBrowserPanel.SetActive(false);
                _initialPanel.SetActive(true);

                _joinLobbyButton.interactable = true;
            });
        }

        if (_backFromHostButton != null)
        {
            _backFromHostButton.onClick.AddListener(() =>
            {
                _hostPanel.SetActive(false);
                _sessionBrowserPanel.SetActive(true);
            });
        }
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
        _hostButton.interactable = false;
        _runnerHandler.HostGame(_inputSessionName.text, "Gameplay");
    }

    private void SalirDelJuego()
    {
        Debug.Log("Cerrando el juego...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
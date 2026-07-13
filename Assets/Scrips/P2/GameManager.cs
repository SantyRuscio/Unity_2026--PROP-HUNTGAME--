using Fusion;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("UI Screens")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _menuButton;

    [Networked, OnChangedRender(nameof(OnGameOverChanged))]
    public NetworkBool IsGameOver { get; set; }

    [Networked]
    public NetworkBool HunterWon { get; set; }

    public override void Spawned()
    {
        Instance = this;
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        if (_menuButton != null) _menuButton.onClick.AddListener(ReturnToMenu);

        if (Runner.IsServer)
        {
            if (_restartButton != null)
            {
                _restartButton.gameObject.SetActive(true);
                _restartButton.onClick.AddListener(RestartLevel);
            }
        }
        else
        {
            if (_restartButton != null) _restartButton.gameObject.SetActive(false);
        }
    }

    public void EndGame(bool hunterWon)
    {
        if (!Runner.IsServer) return;

        HunterWon = hunterWon;
        IsGameOver = true;
        var doorTimer = FindFirstObjectByType<DoorTimer>();
        if (doorTimer != null)
        {
            doorTimer.MatchEnded = true;
        }
    }

    private void OnGameOverChanged()
    {
        if (IsGameOver) ShowGameOverScreen();
        else HideGameOverScreen();
    }

    private void ShowGameOverScreen()
    {
        if (_gameOverPanel == null || _resultText == null) return;

        _gameOverPanel.SetActive(true);

        bool amIHunter = false;
        foreach (var player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            if (player.Object != null && player.Object.HasInputAuthority)
            {
                amIHunter = player.IsHunter;
                break;
            }
        }

        bool didIWin = (amIHunter && HunterWon) || (!amIHunter && !HunterWon);

        if (didIWin)
        {
            _resultText.text = "VICTORY!";
            _resultText.color = Color.green;
        }
        else
        {
            _resultText.text = "DEFEAT!";
            _resultText.color = Color.red;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HideGameOverScreen()
    {
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void RestartLevel()
    {
        if (Runner.IsServer)
        {
            IsGameOver = false;

            var doorTimer = FindFirstObjectByType<DoorTimer>();
            if (doorTimer != null) doorTimer.ResetTimer();

            var spawner = FindFirstObjectByType<PlayerSpawning>();
            if (spawner != null)
            {
                Transform[] spawns = spawner.GetSpawnPoints();
                var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

                if (players.Length > 0 && spawns.Length > 0)
                {
                    int randomHunterIndex = UnityEngine.Random.Range(0, players.Length);
                    int propSpawnIndex = 1;
                    int index = 0;

                    foreach (var p in players)
                    {
                        p.IsHunter = (index == randomHunterIndex);

                        Transform mySpawn;
                        if (p.IsHunter)
                        {
                            mySpawn = spawns[0];
                        }
                        else
                        {
                            if (spawns.Length > 1)
                            {
                                mySpawn = spawns[propSpawnIndex % (spawns.Length - 1) + 1];
                                propSpawnIndex++;
                            }
                            else
                            {
                                mySpawn = spawns[0];
                            }
                        }

                        p.ResetPlayerState(mySpawn.position, mySpawn.rotation);
                        index++;
                    }
                }
            }
        }
    }

    private void ReturnToMenu()
    {
        if (Runner.IsServer) RPC_ReturnToMenuAll();
        else DisconnectAndReturn();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ReturnToMenuAll()
    {
        DisconnectAndReturn();
    }

    private void DisconnectAndReturn()
    {
        Runner.Shutdown();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Elements")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;

    private void Awake()
    {
        Instance = this;
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver(string message)
    {
        gameOverPanel.SetActive(true);
        winnerText.text = message;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Btn_RestartNormal()
    {
        TriggerRestart();
    }


    private void TriggerRestart()
    {
        foreach (var player in FindObjectsByType<Player>(FindObjectsSortMode.None))
        {
            if (player.Object.HasStateAuthority)
            {
                player.RPC_RestartGame();
                break;
            }
        }
    }
}
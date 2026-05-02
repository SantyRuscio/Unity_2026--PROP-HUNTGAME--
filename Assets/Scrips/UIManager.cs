using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Elements")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;

    [Header("Hide Timer UI")]
    public TextMeshProUGUI hideTimerText;

    private void Awake()
    {
        Instance = this;
        gameOverPanel.SetActive(false);

        if (hideTimerText != null)
            hideTimerText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (hideTimerText == null) return;

        Player hunter = BuscarHunter();

        if (hunter == null)
        {
            hideTimerText.gameObject.SetActive(false);
            return;
        }

        if (hunter.HideTimer.IsRunning)
        {
            float tiempo = hunter.HideTimer.RemainingTime(hunter.Runner) ?? 0f;
            int segundos = Mathf.CeilToInt(tiempo);

            hideTimerText.text = "Tiempo De Escondite: " + segundos;

            if (segundos <= 3)
            {
                hideTimerText.color = Color.red;
            }
            else
            {
                hideTimerText.color = Color.white;
            }

            hideTimerText.gameObject.SetActive(true);
        }
        else
        {
            hideTimerText.gameObject.SetActive(false);
        }
    }


    Player BuscarHunter()
    {
        var players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        foreach (var p in players)
        {
            if (p.isHunter)
                return p;
        }

        return null;
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
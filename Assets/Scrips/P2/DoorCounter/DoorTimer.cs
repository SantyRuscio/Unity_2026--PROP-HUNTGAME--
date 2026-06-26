using Fusion;
using TMPro;
using UnityEngine;

public class DoorTimer : NetworkBehaviour
{
    [Header("Configuración de Tiempos")]
    [SerializeField] private float timeToHide = 30f;
    [SerializeField] private float timeToHunt = 120f;

    [Header("UI y Visuales")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject doorVisual;

    [Networked]
    private float CurrentTime { get; set; }

    [Networked, OnChangedRender(nameof(OnPhaseChanged))]
    private NetworkBool IsHuntingPhase { get; set; }

    [Networked]
    private NetworkBool _matchEnded { get; set; }

    public override void Spawned()
    {
        if (Runner.IsServer)
        {
            CurrentTime = timeToHide;
            IsHuntingPhase = false;
            _matchEnded = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.SessionInfo.PlayerCount < 2 || _matchEnded) return;

        if (Runner.IsServer)
        {
            CurrentTime -= Runner.DeltaTime;

            if (CurrentTime <= 0)
            {
                if (!IsHuntingPhase)
                {
                    IsHuntingPhase = true;
                    CurrentTime = timeToHunt;
                }
                else
                {
                    CurrentTime = 0;
                    _matchEnded = true;

                    if (GameManager.Instance != null)
                        GameManager.Instance.TerminarJuego(false);
                }
            }
        }
    }

    public override void Render()
    {
        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(CurrentTime / 60);
            int seconds = Mathf.FloorToInt(CurrentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void ResetTimer()
    {
        if (Runner.IsServer)
        {
            CurrentTime = timeToHide;
            IsHuntingPhase = false;
            _matchEnded = false;
        }
    }

    private void OnPhaseChanged()
    {
        if (IsHuntingPhase)
        {
            if (doorVisual != null) doorVisual.SetActive(false);
            if (timerText != null) timerText.color = Color.red;
        }
        else
        {
            if (doorVisual != null) doorVisual.SetActive(true);
            if (timerText != null) timerText.color = Color.white;
        }
    }
}
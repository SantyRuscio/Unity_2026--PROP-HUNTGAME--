using Fusion;
using TMPro;
using UnityEngine;

public class DoorTimer : NetworkBehaviour
{
    [Header("Configuración de Tiempos")]
    [SerializeField] private int timeToHide = 15;
    [SerializeField] private int timeToHunt = 120;

    [Header("UI y Visuales")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject doorVisual;

    [Networked] public int CurrentTime { get; set; }
    [Networked] public NetworkBool IsHuntingPhase { get; set; }
    [Networked] public NetworkBool MatchEnded { get; set; }

    private TickTimer _secTimer;

    public override void Spawned()
    {
        if (Runner.IsServer)
        {
            CurrentTime = timeToHide;
            IsHuntingPhase = false;
            MatchEnded = false;
            _secTimer = TickTimer.CreateFromSeconds(Runner, 1f);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.SessionInfo.PlayerCount < 2 || MatchEnded) return;

        if (Runner.IsServer)
        {
            if (_secTimer.Expired(Runner))
            {
                CurrentTime--;
                _secTimer = TickTimer.CreateFromSeconds(Runner, 1f);

                if (CurrentTime <= 0)
                {
                    if (!IsHuntingPhase)
                    {
                        IsHuntingPhase = true;
                        CurrentTime = timeToHunt;
                        RPC_UpdateVisuals(true);
                    }
                    else
                    {
                        MatchEnded = true;
                        if (GameManager.Instance != null) GameManager.Instance.TerminarJuego(false);
                    }
                }
            }
        }
    }

    public override void Render()
    {
        if (timerText != null)
        {
            int m = Mathf.Max(0, CurrentTime / 60);
            int s = Mathf.Max(0, CurrentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", m, s);

            if (IsHuntingPhase) timerText.color = Color.red;
            else timerText.color = Color.white;
        }
    }

    public void ResetTimer()
    {
        if (Runner.IsServer)
        {
            CurrentTime = timeToHide;
            IsHuntingPhase = false;
            MatchEnded = false;
            _secTimer = TickTimer.CreateFromSeconds(Runner, 1f);
            RPC_UpdateVisuals(false);
        }
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateVisuals(bool isHunting)
    {
        if (doorVisual != null) doorVisual.SetActive(!isHunting);
    }
}
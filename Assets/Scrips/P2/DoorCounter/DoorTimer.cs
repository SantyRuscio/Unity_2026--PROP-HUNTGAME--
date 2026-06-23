using Fusion;
using TMPro;
using UnityEngine;

public class DoorTimer : NetworkBehaviour
{
    [SerializeField] private float timeToDestroy = 30f; // Tiempo en segundos
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject doorVisual;

    [Networked, OnChangedRender(nameof(OnTimeChanged))]
    private float CurrentTime { get; set; }

    [Networked, OnChangedRender(nameof(OnDoorStateChanged))]
    private NetworkBool IsDoorDestroyed { get; set; }

    public override void Spawned()
    {
        if (Runner.IsServer)
        {
            CurrentTime = timeToDestroy;
            IsDoorDestroyed = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Consigna: No iniciar hasta que haya al menos 2 jugadores conectados
        if (Runner.SessionInfo.PlayerCount < 2) return;

        // Solo el Host maneja el estado del tiempo
        if (Runner.IsServer && !IsDoorDestroyed)
        {
            CurrentTime -= Runner.DeltaTime;

            if (CurrentTime <= 0)
            {
                CurrentTime = 0;
                IsDoorDestroyed = true; // Esto disparará la desactivación en todos lados
            }
        }
    }

    private void OnTimeChanged()
    {
        UpdateTimerUI();
    }

    private void OnDoorStateChanged()
    {
        HandleDoorDestruction();
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(CurrentTime).ToString();
        }
    }

    private void HandleDoorDestruction()
    {
        if (IsDoorDestroyed)
        {
            if (doorVisual != null) doorVisual.SetActive(false);
            if (timerText != null) timerText.gameObject.SetActive(false);

            Debug.Log("¡La puerta se ha abierto! ¡Hunters al ataque!");
        }
    }
}

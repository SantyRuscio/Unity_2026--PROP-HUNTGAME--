using Fusion;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("UI Pantallas")]
    [SerializeField] private GameObject _panelGameOver;
    [SerializeField] private TextMeshProUGUI _textoResultado;
    [SerializeField] private Button _btnReiniciar;
    [SerializeField] private Button _btnMenu;

    [Networked, OnChangedRender(nameof(OnGameOverChanged))]
    public NetworkBool IsGameOver { get; set; }

    [Networked]
    public NetworkBool HunterWon { get; set; }

    public override void Spawned()
    {
        Instance = this;
        if (_panelGameOver != null) _panelGameOver.SetActive(false);
        if (_btnMenu != null) _btnMenu.onClick.AddListener(VolverAlMenu);

        if (Runner.IsServer)
        {
            if (_btnReiniciar != null)
            {
                _btnReiniciar.gameObject.SetActive(true);
                _btnReiniciar.onClick.AddListener(ReiniciarNivel);
            }
        }
        else
        {
            if (_btnReiniciar != null) _btnReiniciar.gameObject.SetActive(false);
        }
    }

    public void TerminarJuego(bool ganoHunter)
    {
        if (!Runner.IsServer) return;

        HunterWon = ganoHunter;
        IsGameOver = true;
    }

    private void OnGameOverChanged()
    {
        if (IsGameOver) MostrarPantallaFinal();
        else OcultarPantallaFinal();
    }

    private void MostrarPantallaFinal()
    {
        if (_panelGameOver == null || _textoResultado == null) return;

        _panelGameOver.SetActive(true);

        bool soyHunter = false;
        foreach (var player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            if (player.Object != null && player.Object.HasInputAuthority)
            {
                soyHunter = player.IsHunter;
                break;
            }
        }

        bool gane = (soyHunter && HunterWon) || (!soyHunter && !HunterWon);

        if (gane)
        {
            _textoResultado.text = "¡VICTORIA!";
            _textoResultado.color = Color.green;
        }
        else
        {
            _textoResultado.text = "¡DERROTA!";
            _textoResultado.color = Color.red;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OcultarPantallaFinal()
    {
        if (_panelGameOver != null) _panelGameOver.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ReiniciarNivel()
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

                foreach (var p in players)
                {
                    Transform miSpawn = p.IsHunter ? spawns[0] : spawns[1];
                    p.ResetPlayerState(miSpawn.position, miSpawn.rotation);
                }
            }
        }
    }

    private void VolverAlMenu()
    {
        if (Runner.IsServer) RPC_VolverAlMenuTodos();
        else Runner.Shutdown();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_VolverAlMenuTodos()
    {
        Runner.Shutdown();
    }
}
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

            // Resetear el Timer de la puerta
            var doorTimer = FindFirstObjectByType<DoorTimer>();
            if (doorTimer != null) doorTimer.ResetTimer();

            var spawner = FindFirstObjectByType<PlayerSpawning>();
            if (spawner != null)
            {
                Transform[] spawns = spawner.GetSpawnPoints();
                var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

                if (players.Length > 0 && spawns.Length > 0)
                {
                    // 1. Elegimos al azar quién va a ser el Cazador
                    int indiceHunterAleatorio = UnityEngine.Random.Range(0, players.Length);

                    // Índices para repartir los spawns de los Props adentro de la casa
                    // Asumimos que el spawn [0] es para el Hunter (afuera) y del [1] en adelante son para Props
                    int propSpawnIndex = 1;

                    int index = 0;
                    foreach (var p in players)
                    {
                        // 2. Asignamos el rol
                        bool asignadoComoHunter = (index == indiceHunterAleatorio);
                        p.IsHunter = asignadoComoHunter;

                        // 3. SELECCIÓN DE SPAWN BASADO EN EL ROL ACTUAL
                        Transform miSpawn;

                        if (p.IsHunter)
                        {
                            // Si es el cazador, va sí o sí al primer spawn (afuera de la casa)
                            miSpawn = spawns[0];
                        }
                        else
                        {
                            // Si es un objeto, va a los spawns siguientes (adentro de la casa)
                            // Usamos el % por si hay más jugadores que puntos de spawn de props creados
                            if (spawns.Length > 1)
                            {
                                miSpawn = spawns[propSpawnIndex % (spawns.Length - 1) + 1];
                                propSpawnIndex++;
                            }
                            else
                            {
                                miSpawn = spawns[0]; // Backup por si solo armaste 1 spawn en total
                            }
                        }

                        // 4. Resetear estado y teletransportar al lugar correcto
                        p.ResetPlayerState(miSpawn.position, miSpawn.rotation);

                        index++;
                    }
                }
            }
        }
    }

    private void VolverAlMenu()
    {
        if (Runner.IsServer)
        {
            RPC_VolverAlMenuTodos();
        }
        else
        {
            DesconectarYVolver();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_VolverAlMenuTodos()
    {
        DesconectarYVolver();
    }

    private void DesconectarYVolver()
    {
        Runner.Shutdown();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
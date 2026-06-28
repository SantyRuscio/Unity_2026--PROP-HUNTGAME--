using Fusion;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerMov))]
[RequireComponent(typeof(PlayerWeapon))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : NetworkBehaviour
{
    private PlayerMov _playerMov;
    private PlayerWeapon _weapon;
    private MovementAnimation _anim;
    private AudioSource _audioSource;

    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private AudioClip whistleSound;

    [Header("Configuración del Silbido")]
    [SerializeField] private float whistleCooldown = 5f;
    [SerializeField] private Image whistleUIIcon;

    [Networked] public NetworkBool IsHunter { get; set; }
    [Networked] public int CurrentPropID { get; set; }
    [Networked] public NetworkBool IsFrozen { get; set; }

    [Networked] private TickTimer WhistleCooldownTimer { get; set; }

    private int _maxProps = 3;

    private void Awake()
    {
        _playerMov = GetComponent<PlayerMov>();
        _weapon = GetComponent<PlayerWeapon>();
        _anim = GetComponentInChildren<MovementAnimation>();
        _audioSource = GetComponent<AudioSource>();

        _audioSource.spatialBlend = 1f;
        _audioSource.maxDistance = 20f;
        _audioSource.loop = false;
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.SessionInfo.PlayerCount < 2) return;
        if (!GetInput(out NetworkInputData inputs)) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        // La rotación horizontal de la cámara se aplica siempre que el jugador no esté congelado
        if (!IsFrozen)
        {
            transform.Rotate(Vector3.up * inputs.lookYaw * sensitivity);
        }

        Vector3 dir = new Vector3(inputs.moveAxis.x, 0, inputs.moveAxis.y);

        // Control del movimiento base horizontal
        if (IsFrozen)
        {
            _playerMov.Move(Vector3.zero);
        }
        else
        {
            _playerMov.Move(dir);

            if (inputs.Buttons.IsSet(ButtonTypes.Jump))
                _playerMov.Jump();
        }

        // ========================================================
        // CONTROL EXCLUSIVO POR ROLES
        // ========================================================
        if (IsHunter)
        {
            // El cazador puede disparar
            if (inputs.Buttons.IsSet(ButtonTypes.Shot))
            {
                _weapon.ShootRaycast();
                if (_anim != null) _anim.TriggerShoot();
            }
        }
        else
        {
            // El objeto (Prop) puede usar sus habilidades si no está congelado en el piso
            if (!IsFrozen)
            {
                if (inputs.Buttons.IsSet(ButtonTypes.Transform))
                {
                    CycleProp();
                }

                if (inputs.Buttons.IsSet(ButtonTypes.Whistle))
                {
                    if (WhistleCooldownTimer.ExpiredOrNotRunning(Runner))
                    {
                        WhistleCooldownTimer = TickTimer.CreateFromSeconds(Runner, whistleCooldown);

                        if (Object.HasInputAuthority)
                        {
                            RPC_PlayWhistle();
                        }
                    }
                }
            }

            // Alternar estado de congelación (Funciona siempre para el Prop)
            if (inputs.Buttons.IsSet(ButtonTypes.Freeze))
            {
                IsFrozen = !IsFrozen;
            }
        }
    }

    public override void Render()
    {
        // Doble validación de seguridad: Si no soy el dueño local, no toco la UI
        if (!Object.HasInputAuthority || whistleUIIcon == null) return;

        if (!IsHunter)
        {
            //Me aseguro de prender el ícono si estaba apagado
            if (!whistleUIIcon.gameObject.activeSelf)
                whistleUIIcon.gameObject.SetActive(true);

            // Control de opacidad por Cooldown
            if (WhistleCooldownTimer.IsRunning)
            {
                float remainingTime = WhistleCooldownTimer.RemainingTime(Runner) ?? 0f;
                float progress = remainingTime / whistleCooldown;
                float alpha = Mathf.Lerp(1f, 0.3f, progress);

                Color c = whistleUIIcon.color;
                whistleUIIcon.color = new Color(c.r, c.g, c.b, alpha);
            }
            else
            {
                Color c = whistleUIIcon.color;
                whistleUIIcon.color = new Color(c.r, c.g, c.b, 1f);
            }
        }
        else
        {
            // SI SOY HUNTER: Apago el ícono inmediatamente para que nunca aparezca en mi pantalla
            if (whistleUIIcon.gameObject.activeSelf)
                whistleUIIcon.gameObject.SetActive(false);
        }
    }

    void CycleProp()
    {
        CurrentPropID++;
        if (CurrentPropID > _maxProps)
            CurrentPropID = 0;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_PlayWhistle()
    {
        if (_audioSource != null && whistleSound != null)
        {
            _audioSource.PlayOneShot(whistleSound);
        }
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            GameObject findIcon = GameObject.Find("Icono_Silbato");
            if (findIcon != null)
            {
                whistleUIIcon = findIcon.GetComponent<Image>();
            }
        }
    }

    public void ResetPlayerState(Vector3 pos, Quaternion rot)
    {
        if (Runner.IsServer)
        {
            CurrentPropID = 0;
            IsFrozen = false; //Descongelamos al jugador para que inicie con movilidad completa

            var health = GetComponent<HealthComponent>();
            if (health != null) health.ResetHealth();

            RPC_Teleport(pos, rot);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_Teleport(Vector3 pos, Quaternion rot)
    {
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        transform.position = pos;
        transform.rotation = rot;

        if (_playerMov != null)
        {
            _playerMov.Teleport(pos);
        }
        if (cc != null) cc.enabled = true;
    }
}
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

    [Header("Prop Settings")]
    [SerializeField] private float whistleCooldown = 5f;
    private Image whistleUIIcon;

    [Header("Hunter Settings")]
    [SerializeField] private float shootCooldown = 3f;
    [SerializeField] private int maxAmmo = 10;
    private Image gunUIIcon;

    [Networked] public NetworkBool IsHunter { get; set; }
    [Networked] public int CurrentPropID { get; set; }
    [Networked] public NetworkBool IsFrozen { get; set; }

    [Networked] public float CameraPitch { get; set; }
    [Networked] public float CameraYaw { get; set; }

    [Networked] private TickTimer WhistleCooldownTimer { get; set; }
    [Networked] private TickTimer ShootCooldownTimer { get; set; }
    [Networked] public int CurrentAmmo { get; set; }

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

        CameraPitch -= inputs.lookPitch * sensitivity;
        CameraPitch = Mathf.Clamp(CameraPitch, -85f, 85f);

        if (!IsFrozen)
        {
            transform.Rotate(Vector3.up * inputs.lookYaw * sensitivity);
            CameraYaw = transform.eulerAngles.y;
        }
        else
        {
            CameraYaw += inputs.lookYaw * sensitivity;
        }

        Vector3 dir = new Vector3(inputs.moveAxis.x, 0, inputs.moveAxis.y);
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

        if (IsHunter)
        {
            if (inputs.Buttons.IsSet(ButtonTypes.Shot))
            {
                if (CurrentAmmo > 0 && ShootCooldownTimer.ExpiredOrNotRunning(Runner))
                {
                    ShootCooldownTimer = TickTimer.CreateFromSeconds(Runner, shootCooldown);
                    CurrentAmmo--;

                    _weapon.ShootRaycast();
                    RPC_PlayShootAnimation();
                }
            }
        }
        else
        {
            if (!IsFrozen)
            {
                if (inputs.Buttons.IsSet(ButtonTypes.Transform))
                {
                    CycleProp();
                }
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

            if (inputs.Buttons.IsSet(ButtonTypes.Freeze))
            {
                IsFrozen = !IsFrozen;
            }
        }
    }

    public override void Render()
    {
        if (!Object.HasInputAuthority) return;

        if (!IsHunter)
        {
            if (gunUIIcon != null && gunUIIcon.gameObject.activeSelf) gunUIIcon.gameObject.SetActive(false);

            if (whistleUIIcon != null)
            {
                if (!whistleUIIcon.gameObject.activeSelf) whistleUIIcon.gameObject.SetActive(true);

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
        }
        else
        {
            if (whistleUIIcon != null && whistleUIIcon.gameObject.activeSelf) whistleUIIcon.gameObject.SetActive(false);

            if (gunUIIcon != null)
            {
                if (!gunUIIcon.gameObject.activeSelf) gunUIIcon.gameObject.SetActive(true);

                if (CurrentAmmo <= 0)
                {
                    Color c = gunUIIcon.color;
                    gunUIIcon.color = new Color(c.r, c.g, c.b, 0.3f); 
                }
                else if (ShootCooldownTimer.IsRunning)
                {
                    float remainingTime = ShootCooldownTimer.RemainingTime(Runner) ?? 0f;
                    float progress = remainingTime / shootCooldown;
                    float alpha = Mathf.Lerp(1f, 0.3f, progress);

                    Color c = gunUIIcon.color;
                    gunUIIcon.color = new Color(c.r, c.g, c.b, alpha); 
                }
                else
                {
                    Color c = gunUIIcon.color;
                    gunUIIcon.color = new Color(c.r, c.g, c.b, 1f); 
                }
            }
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

    [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayShootAnimation()
    {
        if (_anim != null) _anim.TriggerShoot();
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            GameObject findIcon = GameObject.Find("Icono_Silbato");
            if (findIcon != null) whistleUIIcon = findIcon.GetComponent<Image>();

            GameObject findGunIcon = GameObject.Find("Icono_Arma");
            if (findGunIcon != null) gunUIIcon = findGunIcon.GetComponent<Image>();
        }

        if (Object.HasStateAuthority)
        {
            CurrentAmmo = maxAmmo;
        }
    }

    public void ResetPlayerState(Vector3 pos, Quaternion rot)
    {
        if (Runner.IsServer)
        {
            CurrentPropID = 0;
            IsFrozen = false;
            CameraPitch = 0f;
            CameraYaw = rot.eulerAngles.y;

            CurrentAmmo = maxAmmo;

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
using UnityEngine;
using Fusion;
public class HideDoors : NetworkBehaviour
{
    [Networked] private bool puertasAbiertas { get; set; }

    private AudioSource audioSource;
    private bool sonidoReproducido = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        Player hunter = BuscarHunter();
        if (hunter == null) return;

        if (Object.HasStateAuthority)
        {
            if (!hunter.HideTimer.IsRunning && !puertasAbiertas)
            {
                puertasAbiertas = true;

                RPC_PlaySound();
            }
        }

        if (puertasAbiertas)
        {
            DesactivarPuertas();
        }
    }

    void DesactivarPuertas()
    {
        foreach (Transform hijo in transform)
        {
            hijo.gameObject.SetActive(false);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_PlaySound()
    {
        if (audioSource != null && !sonidoReproducido)
        {
            audioSource.Play();
            sonidoReproducido = true;
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
}
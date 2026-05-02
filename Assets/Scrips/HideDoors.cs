using UnityEngine;
using Fusion;

public class HideDoors : NetworkBehaviour
{
    [Networked] private bool puertasAbiertas { get; set; }

    void Update()
    {
        Player hunter = BuscarHunter();

        if (hunter == null) return;

        if (Object.HasStateAuthority)
        {
            if (!hunter.HideTimer.IsRunning && !puertasAbiertas)
            {
                puertasAbiertas = true;
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
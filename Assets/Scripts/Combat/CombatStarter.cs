using UnityEngine;

public class CombatStarter : MonoBehaviour
{
    private bool started = false;
    void Start()
    {
        if (started) return;
        started = true;

        Debug.Log($"CombatStarter.Start called on '{gameObject.name}'");

        if (TeamManager.Instance == null)
        {
            Debug.LogError("TeamManager not found");
            return;
        }

        if (CombatSystem.Instance == null)
        {
            Debug.LogError("CombatSystem not found");
            return;
        }

        if (!TeamManager.Instance.EquipoListo())
        {
            Debug.LogError("Team is not ready");
            return;
        }

        var team = TeamManager.Instance.equipoActual;

        if (team.activos[0] == null || team.activos[1] == null)
        {
            Debug.LogError("Team incomplete");
            return;
        }

        // activos[0] = jugador local (lado 1)
        // activos[1] = rival IA (lado -1)
        var hud = GetComponent<CombatHUD>();
        if (hud == null)
            hud = gameObject.AddComponent<CombatHUD>();

        hud.Setup(team.activos[0], team.activos[1]);

        CombatSystem.Instance.StartMatch1v1(
            team.activos[0],
            team.activos[1]);
    }
}

using UnityEngine;

public class CombatStarter : MonoBehaviour
{
    void Start()
    {
        if (TeamManager.Instance == null)
        {
            Debug.LogError("TeamManager not found");
            return;
        }

        var team = TeamManager.Instance.equipoActual;

        if (team.activos[0] == null || team.activos[1] == null)
        {
            Debug.LogError("Team incomplete");
            return;
        }

        if (CombatSystem.Instance == null)
        {
            Debug.LogError("CombatSystem not found");
            return;
        }

        // activos[0] = jugador local (lado 1)
        // activos[1] = rival IA (lado -1)
        CombatSystem.Instance.StartMatch1v1(
            team.activos[0],
            team.activos[1]);
    }
}
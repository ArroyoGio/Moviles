using UnityEngine;
using System.Collections.Generic;

public class CombatSystem : MonoBehaviour
{
    public static CombatSystem Instance;

    // lado 1 = jugador local, lado -1 = rival
    private Dictionary<int, List<Fighter>> fighters = new Dictionary<int, List<Fighter>>();

    void Awake()
    {
        Instance = this;
        fighters[1] = new List<Fighter>();
        fighters[-1] = new List<Fighter>();
    }

    public void StartMatch1v1(VeteranData localData, VeteranData rivalData, bool startRound = true)
    {
        // Destruye fighters anteriores correctamente
        List<Fighter> toDestroy = new List<Fighter>();
        foreach (var list in fighters.Values)
            foreach (var f in list)
                if (f != null) toDestroy.Add(f);

        foreach (var f in toDestroy)
            Destroy(f.gameObject);

        fighters[1].Clear();
        fighters[-1].Clear();

        // Espera un frame antes de instanciar — evita conflictos
        var local = SpawnFighter(localData, new Vector3(-2f, 0, 0));
        var rival = SpawnFighter(rivalData, new Vector3(2f, 0, 0));

        fighters[1].Add(local);
        fighters[-1].Add(rival);

        local.Initialize(localData, 1);
        rival.Initialize(rivalData, -1);

        if (startRound)
            ArenaManager.Instance.StartRound(1);
    }
    Fighter SpawnFighter(VeteranData data, Vector3 pos)
    {
        var go = Instantiate(data.baseData.combatPrefab, pos, Quaternion.identity);
        return go.GetComponent<Fighter>();
    }

    public void NotifyKO(Fighter fallen)
    {
        Debug.Log($"KO: {fallen.name} lado {fallen.side}");

        if (fighters.ContainsKey(fallen.side))
            fighters[fallen.side].Remove(fallen);

        Debug.Log($"Lado 1: {fighters[1].Count} — Lado -1: {fighters[-1].Count}");

        ArenaManager.Instance.RoundEndByKO();
    }

    public List<Fighter> GetActiveRivals(int side)
    {
        int rivalSide = side == 1 ? -1 : 1;
        return fighters.ContainsKey(rivalSide) ? fighters[rivalSide] : new List<Fighter>();
    }

    public List<Fighter> GetActiveAllies(int side)
    {
        return fighters.ContainsKey(side) ? fighters[side] : new List<Fighter>();
    }
}
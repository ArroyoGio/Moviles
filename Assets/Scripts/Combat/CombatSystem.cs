using UnityEngine;
using System.Collections.Generic;
using System;

public class CombatSystem : MonoBehaviour
{
    public static CombatSystem Instance;
    public static event Action<Fighter, Fighter> OnFightersReady;

    // lado 1 = jugador local, lado -1 = rival
    private Dictionary<int, List<Fighter>> fighters = new Dictionary<int, List<Fighter>>();
    private Transform spawnRoot;

    void Awake()
    {
        // Evita duplicados si hay varias instancias en la escena
        if (Instance != null && Instance != this)
        {
            Debug.Log($"CombatSystem: instancia duplicada detectada en '{gameObject.name}', destruyendo la nueva.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Crea un contenedor para los fighters instanciados en combate
        var existing = transform.Find("SpawnedFighters");
        if (existing != null) spawnRoot = existing;
        else
        {
            var go = new GameObject("SpawnedFighters");
            go.transform.SetParent(transform);
            spawnRoot = go.transform;
        }
        fighters[1] = new List<Fighter>();
        fighters[-1] = new List<Fighter>();
    }

    public void StartMatch1v1(VeteranData localData, VeteranData rivalData, bool startRound = true)
    {
        Debug.Log($"CombatSystem.StartMatch1v1 called on '{gameObject.name}'. Current counts -> side1: {fighters[1].Count} side-1: {fighters[-1].Count}");

        // Destruye fighters anteriores correctamente (limpia el contenedor de spawned)
        if (spawnRoot != null)
        {
            for (int i = spawnRoot.childCount - 1; i >= 0; i--)
            {
                var child = spawnRoot.GetChild(i);
                Destroy(child.gameObject);
            }
        }

        // Asegura que la lista interna este limpia
        foreach (var key in new List<int>(fighters.Keys))
            fighters[key].Clear();

        // Log fighters que existen en la escena pero que NO pertenecen al contenedor (posibles originales)
        var all = FindObjectsOfType<Fighter>();
        foreach (var f in all)
        {
            if (f.transform.parent != spawnRoot)
                Debug.Log($"CombatSystem: fighter presente en escena fuera del contenedor: {f.name} (parent={f.transform.parent?.name})");
        }

        // Instancia los fighters seleccionados desde TeamManager
        var local = SpawnFighter(localData, new Vector3(-3f, 0, 0), "Local");
        var rival = SpawnFighter(rivalData, new Vector3(3f, 0, 0), "Rival");

        if (local == null || rival == null)
        {
            Debug.LogError("CombatSystem: no se pudo iniciar el combate porque falta un Fighter valido.");
            return;
        }

        fighters[1].Add(local);
        fighters[-1].Add(rival);

        local.Initialize(localData, 1);
        rival.Initialize(rivalData, -1);
        OnFightersReady?.Invoke(local, rival);

        if (startRound)
        {
            if (ArenaManager.Instance == null)
            {
                Debug.LogError("CombatSystem: ArenaManager not found.");
                return;
            }

            ArenaManager.Instance.StartRound(1);
        }
    }
    Fighter SpawnFighter(VeteranData data, Vector3 pos, string label)
    {
        if (data == null)
        {
            Debug.LogError("SpawnFighter: VeteranData es null.");
            return null;
        }

        if (data.baseData == null)
        {
            Debug.LogError($"SpawnFighter: '{data.name}' no tiene baseData.");
            return null;
        }

        if (data.baseData.combatPrefab == null)
        {
            Debug.LogError($"SpawnFighter: '{data.baseData.characterName}' no tiene combatPrefab asignado.");
            return null;
        }

        var go = Instantiate(data.baseData.combatPrefab, pos, Quaternion.identity, spawnRoot);
        go.name = $"{label}_{data.baseData.characterName}";
        go.transform.position = pos;
        Debug.Log($"SpawnFighter: instanciado '{go.name}' como hijo de '{spawnRoot.name}'");

        var fighter = go.GetComponent<Fighter>();
        if (fighter == null)
        {
            Debug.LogError($"SpawnFighter: el prefab '{go.name}' no tiene componente Fighter.");
            Destroy(go);
            return null;
        }

        return fighter;
    }

    public void NotifyKO(Fighter fallen)
    {
        Debug.Log($"KO: {fallen.name} lado {fallen.side}");

        if (fighters.ContainsKey(fallen.side))
            fighters[fallen.side].Remove(fallen);

        Debug.Log($"Lado 1: {fighters[1].Count} - Lado -1: {fighters[-1].Count}");

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

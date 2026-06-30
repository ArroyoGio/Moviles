using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrainingManager : MonoBehaviour
{
    public const int MaxTurns = 12;
    public const int MaxEnergy = 20;

    public List<VeteranData> veterans = new List<VeteranData>();

    public VeteranData SelectedVeteran { get; private set; }
    public int TurnsUsed { get; private set; }
    public int EnergyUsed { get; private set; }
    public bool IsComplete => TurnsUsed >= MaxTurns || EnergyUsed >= MaxEnergy;

    public event Action OnStateChanged;
    public event Action<string> OnMessage;
    public event Action<VeteranData, TrainingStatType, float, int, int> OnTrainingCompleted;

    void Awake()
    {
        EnsureVeteransLoaded();
    }

    public void EnsureVeteransLoaded()
    {
        if (veterans == null)
            veterans = new List<VeteranData>();

        veterans.RemoveAll(veteran => veteran == null || veteran.baseData == null);
        if (veterans.Count > 0)
        {
            SortVeteransForTraining();
            return;
        }

#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:VeteranData", new[] { "Assets/ScriptableObjects/Veterans" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            VeteranData veteran = AssetDatabase.LoadAssetAtPath<VeteranData>(path);
            if (veteran != null && veteran.baseData != null)
                veterans.Add(veteran);
        }
#else
        Debug.LogError("TrainingManager no tiene VeteranData asignados.");
#endif

        SortVeteransForTraining();
    }

    private void SortVeteransForTraining()
    {
        string[] order = { "Dot", "Hana", "Caio", "Dakota", "Denis", "Luca", "Omer", "Rio", "Ursula", "Xiao" };
        veterans.Sort((a, b) =>
        {
            int aIndex = Array.IndexOf(order, a.baseData.characterName);
            int bIndex = Array.IndexOf(order, b.baseData.characterName);
            if (aIndex < 0) aIndex = order.Length;
            if (bIndex < 0) bIndex = order.Length;
            int orderCompare = aIndex.CompareTo(bIndex);
            return orderCompare != 0 ? orderCompare : string.Compare(a.baseData.characterName, b.baseData.characterName, StringComparison.Ordinal);
        });
    }

    public void SelectVeteran(VeteranData veteran)
    {
        if (veteran == null || veteran.baseData == null) return;

        SelectedVeteran = veteran;
        TrainingStateManager.GetOrCreate().RestoreTraining(SelectedVeteran);
        EquipmentStateManager.GetOrCreate().RestoreEquipment(SelectedVeteran);
        SelectedVeteran.RecalculateStatsFromBaseAndEquipment();
        OnMessage?.Invoke("Seleccionado: " + SelectedVeteran.baseData.characterName);
        OnStateChanged?.Invoke();
    }

    public bool Train(TrainingStatType stat)
    {
        if (SelectedVeteran == null)
        {
            OnMessage?.Invoke("Selecciona un veterano");
            return false;
        }

        if (IsComplete)
        {
            OnMessage?.Invoke("Entrenamiento completado");
            return false;
        }

        int cost = GetEnergyCost(stat);
        if (EnergyUsed + cost > MaxEnergy)
        {
            OnMessage?.Invoke("Energia insuficiente");
            return false;
        }

        float trainedAmount = ApplyTraining(stat);
        TrainingStateManager.GetOrCreate().SaveTraining(SelectedVeteran);
        SelectedVeteran.RecalculateStatsFromBaseAndEquipment();
        EnergyUsed += cost;
        TurnsUsed++;
        SelectedVeteran.ResetCombatState();

        if (IsComplete)
            OnMessage?.Invoke("Entrenamiento completado");
        else
            OnMessage?.Invoke(GetStatName(stat) + " entrenado");

        OnTrainingCompleted?.Invoke(SelectedVeteran, stat, trainedAmount, TurnsUsed, MaxEnergy - EnergyUsed);
        OnStateChanged?.Invoke();
        return true;
    }

    public int GetEnergyCost(TrainingStatType stat)
    {
        switch (stat)
        {
            case TrainingStatType.Life:
            case TrainingStatType.Stamina:
                return 1;
            case TrainingStatType.Evasion:
            case TrainingStatType.Agility:
            case TrainingStatType.Defense:
                return 2;
            default:
                return 3;
        }
    }

    public string GetStatName(TrainingStatType stat)
    {
        switch (stat)
        {
            case TrainingStatType.Life: return "Vida";
            case TrainingStatType.Stamina: return "Stamina";
            case TrainingStatType.Evasion: return "Evasion";
            case TrainingStatType.Agility: return "Agilidad";
            case TrainingStatType.Defense: return "Defensa";
            case TrainingStatType.Damage: return "Dano";
            case TrainingStatType.Luck: return "Suerte";
            case TrainingStatType.CritMultiplier: return "Critico";
            case TrainingStatType.Push: return "Empuje";
            default: return stat.ToString();
        }
    }

    private float ApplyTraining(TrainingStatType stat)
    {
        float amount = 0f;
        switch (stat)
        {
            case TrainingStatType.Life:
                amount = UnityEngine.Random.Range(4, 9);
                SelectedVeteran.trainingLife += Mathf.RoundToInt(amount);
                break;
            case TrainingStatType.Stamina:
                amount = UnityEngine.Random.Range(4, 9);
                SelectedVeteran.trainingStamina += Mathf.RoundToInt(amount);
                break;
            case TrainingStatType.Evasion:
                amount = UnityEngine.Random.Range(0.03f, 0.0601f);
                SelectedVeteran.trainingEvasion += amount;
                break;
            case TrainingStatType.Agility:
                amount = UnityEngine.Random.Range(3, 8);
                SelectedVeteran.trainingAgility += Mathf.RoundToInt(amount);
                break;
            case TrainingStatType.Defense:
                amount = UnityEngine.Random.Range(0.02f, 0.0501f);
                SelectedVeteran.trainingDefense += amount;
                break;
            case TrainingStatType.Damage:
                amount = UnityEngine.Random.Range(3, 8);
                SelectedVeteran.trainingDamage += Mathf.RoundToInt(amount);
                break;
            case TrainingStatType.Luck:
                amount = UnityEngine.Random.Range(0.02f, 0.0501f);
                SelectedVeteran.trainingLuck += amount;
                break;
            case TrainingStatType.CritMultiplier:
                amount = UnityEngine.Random.Range(0.05f, 0.1001f);
                SelectedVeteran.trainingCritMultiplier += amount;
                break;
            case TrainingStatType.Push:
                amount = UnityEngine.Random.Range(3, 8);
                SelectedVeteran.trainingPush += Mathf.RoundToInt(amount);
                break;
        }

        return amount;
    }
}

public enum TrainingStatType
{
    Life,
    Stamina,
    Evasion,
    Agility,
    Defense,
    Damage,
    Luck,
    CritMultiplier,
    Push
}

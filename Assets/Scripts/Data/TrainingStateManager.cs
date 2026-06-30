using System.Collections.Generic;
using UnityEngine;

public class TrainingStateManager : MonoBehaviour
{
    public static TrainingStateManager Instance { get; private set; }

    private readonly Dictionary<string, TrainingState> trainingByVeteran = new Dictionary<string, TrainingState>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static TrainingStateManager GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        var go = new GameObject("TrainingStateManager");
        return go.AddComponent<TrainingStateManager>();
    }

    public void SaveTraining(VeteranData veteran)
    {
        string key = GetKey(veteran);
        if (string.IsNullOrEmpty(key)) return;

        trainingByVeteran[key] = new TrainingState
        {
            bonusTrainingLife = veteran.trainingLife,
            bonusTrainingDamage = veteran.trainingDamage,
            bonusTrainingDefense = veteran.trainingDefense,
            bonusTrainingAgility = veteran.trainingAgility,
            bonusTrainingLuck = veteran.trainingLuck,
            bonusTrainingEvasion = veteran.trainingEvasion,
            bonusTrainingStamina = veteran.trainingStamina,
            bonusTrainingPush = veteran.trainingPush,
            bonusTrainingCritMultiplier = veteran.trainingCritMultiplier
        };
    }

    public void RestoreTraining(VeteranData veteran)
    {
        string key = GetKey(veteran);
        if (string.IsNullOrEmpty(key)) return;

        if (!trainingByVeteran.TryGetValue(key, out TrainingState state))
            return;

        veteran.trainingLife = state.bonusTrainingLife;
        veteran.trainingDamage = state.bonusTrainingDamage;
        veteran.trainingDefense = state.bonusTrainingDefense;
        veteran.trainingAgility = state.bonusTrainingAgility;
        veteran.trainingLuck = state.bonusTrainingLuck;
        veteran.trainingEvasion = state.bonusTrainingEvasion;
        veteran.trainingStamina = state.bonusTrainingStamina;
        veteran.trainingPush = state.bonusTrainingPush;
        veteran.trainingCritMultiplier = state.bonusTrainingCritMultiplier;
    }

    private string GetKey(VeteranData veteran)
    {
        if (veteran == null) return string.Empty;
        if (!string.IsNullOrEmpty(veteran.veteranId)) return veteran.veteranId;

        if (veteran.baseData != null && !string.IsNullOrEmpty(veteran.baseData.characterName))
            return veteran.baseData.characterName;

        return veteran.name;
    }

    private struct TrainingState
    {
        public int bonusTrainingLife;
        public int bonusTrainingDamage;
        public float bonusTrainingDefense;
        public int bonusTrainingAgility;
        public float bonusTrainingLuck;
        public float bonusTrainingEvasion;
        public int bonusTrainingStamina;
        public int bonusTrainingPush;
        public float bonusTrainingCritMultiplier;
    }
}

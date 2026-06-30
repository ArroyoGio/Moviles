using System.Collections.Generic;
using UnityEngine;

public class EquipmentStateManager : MonoBehaviour
{
    public static EquipmentStateManager Instance { get; private set; }

    private readonly Dictionary<string, EquipmentState> equipmentByVeteran = new Dictionary<string, EquipmentState>();

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

    public static EquipmentStateManager GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        var go = new GameObject("EquipmentStateManager");
        return go.AddComponent<EquipmentStateManager>();
    }

    public void SaveEquipment(VeteranData veteran)
    {
        string key = GetKey(veteran);
        if (string.IsNullOrEmpty(key)) return;

        equipmentByVeteran[key] = new EquipmentState
        {
            weaponSlot = veteran.weaponSlot,
            protectionSlot = veteran.protectionSlot,
            accessorySlot = veteran.accessorySlot,
            consumableSlot = veteran.consumableSlot
        };
    }

    public void RestoreEquipment(VeteranData veteran)
    {
        string key = GetKey(veteran);
        if (string.IsNullOrEmpty(key)) return;

        if (!equipmentByVeteran.TryGetValue(key, out EquipmentState state))
            return;

        veteran.weaponSlot = state.weaponSlot;
        veteran.protectionSlot = state.protectionSlot;
        veteran.accessorySlot = state.accessorySlot;
        veteran.consumableSlot = state.consumableSlot;
    }

    private string GetKey(VeteranData veteran)
    {
        if (veteran == null) return string.Empty;
        if (!string.IsNullOrEmpty(veteran.veteranId)) return veteran.veteranId;

        if (veteran.baseData != null && !string.IsNullOrEmpty(veteran.baseData.characterName))
            return veteran.baseData.characterName;

        return veteran.name;
    }

    private struct EquipmentState
    {
        public ItemData weaponSlot;
        public ItemData protectionSlot;
        public ItemData accessorySlot;
        public ItemData consumableSlot;
    }
}

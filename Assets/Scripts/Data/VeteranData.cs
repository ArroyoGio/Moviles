using UnityEngine;

[CreateAssetMenu(fileName = "NewVeteran", menuName = "PackAPunch/VeteranData")]
public class VeteranData : ScriptableObject
{
    [Header("Referencia base")]
    public CharacterData baseData;
    public string veteranId;  // ej: "dot_001"

    [Header("Stats finales — modificados por entrenamiento")]
    public int life;
    public float defense;
    public int damage;
    public float critMultiplier;
    public float luck;
    public int agility;
    public float evasion;
    public int push;
    public int stamina;

    [Header("Pasiva desbloqueada por entrenamiento")]
    public string passiveName;
    [TextArea] public string passiveEffect;

    [Header("Equipamiento — 4 slots según GDD sección 5")]
    public ItemData weaponSlot;       // Slot 1 — Arma / Vendas ofensivas
    public ItemData protectionSlot;   // Slot 2 — Protección
    public ItemData accessorySlot;    // Slot 3 — Accesorio
    public ItemData consumableSlot;   // Slot 4 — Consumible

    [Header("Estado de combate — solo runtime, no editar en Inspector")]
    [HideInInspector] public int currentLife;
    [HideInInspector] public float currentStamina;
    [HideInInspector] public bool isKO = false;

    // Copia los stats base al VeteranData (para Fase 1 sin entrenamiento aún)
    public void InitFromBase()
    {
        if (baseData == null) return;
        life = baseData.life;
        defense = baseData.defense;
        damage = baseData.damage;
        critMultiplier = baseData.critMultiplier;
        luck = baseData.luck;
        agility = baseData.agility;
        evasion = baseData.evasion;
        push = baseData.push;
        stamina = baseData.stamina;
        passiveName = baseData.passiveName;
        passiveEffect = baseData.passiveEffect;
    }

    public void RecalculateStatsFromBaseAndEquipment()
    {
        if (baseData == null) return;

        InitFromBase();
        ApplyItemBonus(weaponSlot);
        ApplyItemBonus(protectionSlot);
        ApplyItemBonus(accessorySlot);
        ApplyItemBonus(consumableSlot);

        defense = Mathf.Clamp01(defense);
        luck = Mathf.Clamp01(luck);
        evasion = Mathf.Clamp01(evasion);
    }

    private void ApplyItemBonus(ItemData item)
    {
        if (item == null) return;

        life += item.bonusLife;
        damage += item.bonusDamage;
        agility += item.bonusAgility;
        stamina += item.bonusStamina;
        defense += item.bonusDefense;
        evasion += item.bonusEvasion;
        luck += item.bonusCrit;
    }

    // Resetea estado al inicio de cada combate
    public void ResetCombatState()
    {
        currentLife = life;
        currentStamina = stamina;
        isKO = false;
    }
}

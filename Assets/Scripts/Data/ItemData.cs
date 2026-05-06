using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "PackAPunch/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;

    public enum ItemSlot { Weapon, Protection, Accessory, Consumable }
    public ItemSlot slot;

    public enum Rarity { Basic, Sport, Elite }
    public Rarity rarity;

    // Stats que puede dar
    public int bonusDamage;
    public int bonusLife;
    public int bonusAgility;
    public int bonusStamina;
    public float bonusDefense;
    public float bonusEvasion;
    public float bonusCrit;

    [TextArea] public string eliteEffect; // efecto especial si es Elite
}
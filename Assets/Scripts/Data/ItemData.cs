using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "PackAPunch/Item")]
public class ItemData : ScriptableObject
{
    [Header("Identidad")]
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public enum ItemSlot { Weapon, Protection, Accessory, Consumable }
    public ItemSlot slot;
    public enum Rarity { Basic, Sport, Elite }
    public Rarity rarity;

    [Header("Bonus de stats — Slots 1, 2 y 3")]
    public int bonusDamage;
    public int bonusLife;
    public int bonusAgility;
    public int bonusStamina;
    public float bonusDefense;
    public float bonusEvasion;
    public float bonusCrit;
    [TextArea] public string eliteEffect;

    [Header("Consumible — solo si slot == Consumable")]
    public string condicion;
    public string efectoTexto;
    public float efectoValor;
}
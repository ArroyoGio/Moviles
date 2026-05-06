using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "PackAPunch/Character")]
public class CharacterData : ScriptableObject
{
    [Header("Identidad")]
    public string characterName;
    public Sprite portrait;       // imagen de card
    public Sprite fullArt;        // imagen de detalle
    public string animal;
    public string martialArt;

    public enum Role { DPS, Tank, Control, Support, Healer }
    public Role role;

    public enum Gender { Female, Male, Neutral }
    public Gender gender;

    [Header("Stats base")]
    public int life;
    public float defense;         // porcentaje, ej: 0.18f = 18%
    public int damage;
    public float critMultiplier;  // ej: 2.2f
    public float luck;            // porcentaje
    public int agility;
    public float evasion;         // porcentaje
    public int push;
    public int stamina;

    [Header("Habilidades")]
    [TextArea] public string ultiCondition;
    [TextArea] public string ultiEffect;
    public string passiveName;
    [TextArea] public string passiveEffect;

    [Header("Equipamiento actual")]
    public ItemData weaponSlot;
    public ItemData protectionSlot;
    public ItemData accessorySlot;
    public ItemData consumableSlot;
}
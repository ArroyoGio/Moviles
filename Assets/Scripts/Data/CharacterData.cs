using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "PackAPunch/Character")]
public class CharacterData : ScriptableObject
{
    [Header("Identidad")]
    public string characterName;
    public Sprite portrait;
    public Sprite fullArt;
    public GameObject combatPrefab;   // prefab para instanciar en combate
    public string animal;
    public string martialArt;

    public RoleType role;
    public GenderType gender;

    [Header("Stats base — solo lectura, nunca modificar en runtime")]
    public int life;
    public float defense;         // 0.0 a 1.0 — ej: 0.18 = 18%
    public int damage;
    public float critMultiplier;  // ej: 2.2
    public float luck;            // 0.0 a 1.0
    public int agility;
    public float evasion;         // 0.0 a 1.0
    public int push;
    public int stamina;

    [Header("Rango de ataque — definido por arte marcial (GDD 4.9)")]
    // Corto = 1.2u (BJJ, Wrestling, Sambo, Sumo)
    // Medio = 1.4u (Boxeo, Capoeira, Freestyle)
    // Largo = 1.6u (Taekwondo)
    public float attackRange;

    [Header("Habilidades")]
    [TextArea] public string ultiCondition;
    [TextArea] public string ultiEffect;
    public string passiveName;
    [TextArea] public string passiveEffect;

    // NOTA: Los slots de equipamiento NO van aquí.
    // Van en VeteranData — cada copia entrenada tiene su propio equipamiento.
}
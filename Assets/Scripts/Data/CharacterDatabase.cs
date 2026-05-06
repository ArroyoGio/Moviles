using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "PackAPunch/CharacterDatabase")]
public class CharacterDatabase : ScriptableObject
{
    public CharacterData[] allCharacters;
    public ItemData[] allItems;
}
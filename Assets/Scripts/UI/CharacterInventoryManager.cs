using UnityEngine;

public class CharacterInventoryManager : MonoBehaviour
{
    public CharacterDatabase database;
    public GameObject characterCardPrefab;
    public Transform contentParent;

    public static CharacterData selectedCharacter;

    void Start()
    {
        foreach (CharacterData character in database.allCharacters)
        {
            GameObject card = Instantiate(characterCardPrefab, contentParent);
            CharacterCardUI cardUI = card.GetComponent<CharacterCardUI>();
            cardUI.Setup(character);
        }
    }
}
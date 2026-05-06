using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterCardUI : MonoBehaviour
{
    public Image portrait;
    public TMP_Text nameText;
    public TMP_Text martialArtText;
    public TMP_Text roleText;
    public TMP_Text statsText;
    public Image roleTagImage;

    private CharacterData characterData;

    public Color[] roleColors = new Color[]
    {
        new Color(0.91f, 0f, 0.11f),    // DPS - rojo
        new Color(0f, 0.34f, 1f),        // Tank - azul
        new Color(0.55f, 0f, 1f),        // Control - morado
        new Color(0f, 0.77f, 0.31f),     // Support - verde
        new Color(1f, 0.42f, 0f)         // Healer - naranja
    };

    public void Setup(CharacterData data)
    {
        characterData = data;
        if (data.portrait != null)
            portrait.sprite = data.portrait;
        nameText.text = data.characterName;
        martialArtText.text = data.martialArt;
        roleText.text = data.role.ToString();
        statsText.text = $"HP:{data.life}  DMG:{data.damage}";
        roleTagImage.color = roleColors[(int)data.role];

        GetComponent<Button>().onClick.AddListener(() =>
        {
            CharacterInventoryManager.selectedCharacter = data;
            SceneManager.LoadScene("CharacterDetail");
        });
    }
}
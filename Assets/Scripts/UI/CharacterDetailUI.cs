using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterDetailUI : MonoBehaviour
{
    [Header("Info principal")]
    public Image fullArt;
    public TMP_Text nameText;
    public TMP_Text roleText;
    public TMP_Text martialArtText;
    public Image roleTagImage;

    [Header("Stats")]
    public TMP_Text lifeText;
    public TMP_Text damageText;
    public TMP_Text defenseText;
    public TMP_Text agilityText;
    public TMP_Text luckText;
    public TMP_Text evasionText;
    public TMP_Text staminaText;
    public TMP_Text pushText;
    public TMP_Text critText;

    [Header("Habilidades")]
    public TMP_Text ultiText;
    public TMP_Text passiveText;

    public Color[] roleColors = new Color[]
    {
        new Color(0.91f, 0f, 0.11f),
        new Color(0f, 0.34f, 1f),
        new Color(0.55f, 0f, 1f),
        new Color(0f, 0.77f, 0.31f),
        new Color(1f, 0.42f, 0f)
    };

    CharacterData character;

    void Start()
    {
        character = CharacterInventoryManager.selectedCharacter;
        if (character == null)
        {
            SceneManager.LoadScene("CharacterInventory");
            return;
        }
        LoadData();
    }

    public void LoadData()
    {
        if (character.fullArt != null)
            fullArt.sprite = character.fullArt;

        nameText.text = character.characterName;
        roleText.text = character.role.ToString();
        martialArtText.text = character.martialArt;
        roleTagImage.color = roleColors[(int)character.role];

        lifeText.text = $"HP: {character.life}";
        damageText.text = $"DMG: {character.damage}";
        defenseText.text = $"DEF: {character.defense * 100:F0}%";
        agilityText.text = $"AGIL: {character.agility}";
        luckText.text = $"SRT: {character.luck * 100:F0}%";
        evasionText.text = $"EVA: {character.evasion * 100:F0}%";
        staminaText.text = $"STM: {character.stamina}";
        pushText.text = $"EMP: {character.push}";
        critText.text = $"CRIT: {character.critMultiplier}x";

        ultiText.text = $"Ulti\n{character.ultiCondition}\n{character.ultiEffect}";
        passiveText.text = $"Pasiva — {character.passiveName}\n{character.passiveEffect}";
    }

    public void GoBack()
    {
        SceneManager.LoadScene("CharacterInventory");
    }
}
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
        new Color(0.91f, 0f,   0.11f),
        new Color(0f,   0.34f, 1f),
        new Color(0.55f, 0f,   1f),
        new Color(0f,   0.77f, 0.31f),
        new Color(1f,   0.42f, 0f)
    };

    VeteranData veterano;

    void Start()
    {
        veterano = CharacterInventoryManager.selectedVeteran;

        if (veterano == null || veterano.baseData == null)
        {
            SceneManager.LoadScene("CharacterInventory");
            return;
        }

        LoadData();
    }

    void LoadData()
    {
        CharacterData data = veterano.baseData;

        if (data.fullArt != null)
            fullArt.sprite = data.fullArt;

        nameText.text = data.characterName;
        roleText.text = data.role.ToString();
        martialArtText.text = data.martialArt;
        roleTagImage.color = roleColors[(int)data.role];

        // Stats finales del veterano (post entrenamiento)
        lifeText.text = $"HP: {veterano.life}";
        damageText.text = $"DMG: {veterano.damage}";
        defenseText.text = $"DEF: {veterano.defense * 100:F0}%";
        agilityText.text = $"AGIL: {veterano.agility}";
        luckText.text = $"SRT: {veterano.luck * 100:F0}%";
        evasionText.text = $"EVA: {veterano.evasion * 100:F0}%";
        staminaText.text = $"STM: {veterano.stamina}";
        pushText.text = $"EMP: {veterano.push}";
        critText.text = $"CRIT: {veterano.critMultiplier}x";

        // Habilidades vienen del base (nunca cambian)
        ultiText.text = $"Ulti\n{data.ultiCondition}\n{data.ultiEffect}";
        passiveText.text = $"Pasiva — {veterano.passiveName}\n{veterano.passiveEffect}";
    }

    public void GoBack()
    {
        SceneManager.LoadScene("CharacterInventory");
    }
}
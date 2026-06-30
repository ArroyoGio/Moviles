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

    [Header("Slots de equipamiento")]
    public Button btnSlot0;
    public Button btnSlot1;
    public Button btnSlot2;
    public Button btnSlot3;

    [Header("Item Selector")]
    public ItemSelectorUI itemSelectorUI;

    public Color[] roleColors = new Color[]
    {
        new Color(0.91f, 0f, 0.11f),
        new Color(0f, 0.34f, 1f),
        new Color(0.55f, 0f, 1f),
        new Color(0f, 0.77f, 0.31f),
        new Color(1f, 0.42f, 0f)
    };

    private VeteranData veterano;

    void Start()
    {
        veterano = CharacterInventoryManager.selectedVeteran;

        if (veterano == null || veterano.baseData == null)
        {
            SceneManager.LoadScene("CharacterInventory");
            return;
        }

        TrainingStateManager.GetOrCreate().RestoreTraining(veterano);
        EquipmentStateManager.GetOrCreate().RestoreEquipment(veterano);
        veterano.RecalculateStatsFromBaseAndEquipment();
        LoadData();
        InicializarSlots();
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

        RefrescarStats();

        ultiText.text = $"Ulti\n{data.ultiCondition}\n{data.ultiEffect}";
        passiveText.text = $"Pasiva - {veterano.passiveName}\n{veterano.passiveEffect}";

        RefrescarSlots();
    }

    void InicializarSlots()
    {
        btnSlot0.onClick.RemoveAllListeners();
        btnSlot1.onClick.RemoveAllListeners();
        btnSlot2.onClick.RemoveAllListeners();
        btnSlot3.onClick.RemoveAllListeners();

        btnSlot0.onClick.AddListener(() =>
            itemSelectorUI.Abrir(ItemData.ItemSlot.Weapon, veterano, RefrescarSlots));

        btnSlot1.onClick.AddListener(() =>
            itemSelectorUI.Abrir(ItemData.ItemSlot.Protection, veterano, RefrescarSlots));

        btnSlot2.onClick.AddListener(() =>
            itemSelectorUI.Abrir(ItemData.ItemSlot.Accessory, veterano, RefrescarSlots));

        btnSlot3.onClick.AddListener(() =>
            itemSelectorUI.Abrir(ItemData.ItemSlot.Consumable, veterano, RefrescarSlots));
    }

    void RefrescarSlots()
    {
        TrainingStateManager.GetOrCreate().RestoreTraining(veterano);
        veterano.RecalculateStatsFromBaseAndEquipment();
        RefrescarStats();
        ActualizarSlot(btnSlot0, "Weapon", veterano.weaponSlot);
        ActualizarSlot(btnSlot1, "Protection", veterano.protectionSlot);
        ActualizarSlot(btnSlot2, "Accessory", veterano.accessorySlot);
        ActualizarSlot(btnSlot3, "Consumable", veterano.consumableSlot);
    }

    void RefrescarStats()
    {
        lifeText.text = $"HP: {veterano.life}";
        damageText.text = $"DMG: {veterano.damage}";
        defenseText.text = $"DEF: {veterano.defense * 100:F0}%";
        agilityText.text = $"AGIL: {veterano.agility}";
        luckText.text = $"SRT: {veterano.luck * 100:F0}%";
        evasionText.text = $"EVA: {veterano.evasion * 100:F0}%";
        staminaText.text = $"STM: {veterano.stamina}";
        pushText.text = $"EMP: {veterano.push}";
        critText.text = $"CRIT: {veterano.critMultiplier}x";
    }

    void ActualizarSlot(Button button, string slotName, ItemData item)
    {
        if (button == null) return;

        var text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = item != null ? $"{slotName}\n{item.itemName}" : $"{slotName}\nVacio";
            text.color = item != null ? Color.white : new Color(0.76f, 0.78f, 0.82f);
            text.fontSize = item != null ? 21 : 20;
            text.alignment = TextAlignmentOptions.Center;
        }

        var image = button.GetComponent<Image>();
        if (image != null)
            image.color = item != null ? new Color(0.12f, 0.18f, 0.24f, 0.96f) : new Color(0.08f, 0.09f, 0.11f, 0.82f);

        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.22f, 0.32f, 0.42f);
        colors.pressedColor = new Color(0.08f, 0.14f, 0.2f);
        colors.selectedColor = new Color(0.16f, 0.24f, 0.32f);
        colors.disabledColor = new Color(0.18f, 0.18f, 0.18f);
        button.colors = colors;
    }

    public void GoBack()
    {
        SceneManager.LoadScene("CharacterInventory");
    }
}

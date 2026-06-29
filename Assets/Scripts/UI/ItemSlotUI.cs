using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemSlotUI : MonoBehaviour
{
    public Image iconoItem;
    public TMP_Text nombreText;
    public TMP_Text statsText;
    public Image rarezaTag;
    public TMP_Text rarezaText;

    private Button boton;

    public Color colorBasic = Color.gray;
    public Color colorSport = Color.green;
    public Color colorElite = Color.yellow;

    public void Setup(ItemData item,
                      ItemData.ItemSlot tipo,
                      Action<ItemData.ItemSlot, ItemData> callback)
    {
        boton = GetComponent<Button>();

        nombreText.text = item.itemName;

        string stats = "";

        if (item.bonusDamage > 0)
            stats += $"+{item.bonusDamage} DMG ";

        if (item.bonusLife > 0)
            stats += $"+{item.bonusLife} HP ";

        if (item.bonusDefense > 0)
            stats += $"+{item.bonusDefense * 100:F0}% DEF ";

        if (item.bonusAgility > 0)
            stats += $"+{item.bonusAgility} AGI ";

        if (item.bonusStamina > 0)
            stats += $"+{item.bonusStamina} STM ";

        if (item.bonusEvasion > 0)
            stats += $"+{item.bonusEvasion * 100:F0}% EVA ";

        if (item.bonusCrit > 0)
            stats += $"+{item.bonusCrit * 100:F0}% CRIT ";

        statsText.text = string.IsNullOrEmpty(stats) ? "Sin bonus directo" : stats;

        if (item.icon != null)
            iconoItem.sprite = item.icon;

        switch (item.rarity)
        {
            case ItemData.Rarity.Basic:
                rarezaTag.color = colorBasic;
                rarezaText.text = "BASIC";
                break;

            case ItemData.Rarity.Sport:
                rarezaTag.color = colorSport;
                rarezaText.text = "SPORT";
                break;

            case ItemData.Rarity.Elite:
                rarezaTag.color = colorElite;
                rarezaText.text = "ELITE";
                break;
        }

        AplicarEstilo(item);

        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(() =>
        {
            Debug.Log("CLICK " + item.itemName);
            callback?.Invoke(tipo, item);
        });
    }

    void AplicarEstilo(ItemData item)
    {
        var image = GetComponent<Image>();
        if (image != null)
            image.color = new Color(0.11f, 0.13f, 0.16f, 0.96f);

        if (nombreText != null)
        {
            nombreText.color = Color.white;
            nombreText.fontSize = 22;
        }

        if (statsText != null)
        {
            statsText.color = new Color(0.78f, 0.9f, 1f);
            statsText.fontSize = 18;
        }

        if (rarezaTag != null)
            rarezaTag.color = GetRarityColor(item.rarity);

        var colors = boton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.22f, 0.3f, 0.38f);
        colors.pressedColor = new Color(0.08f, 0.12f, 0.17f);
        colors.selectedColor = new Color(0.16f, 0.22f, 0.3f);
        boton.colors = colors;
    }

    Color GetRarityColor(ItemData.Rarity rarity)
    {
        switch (rarity)
        {
            case ItemData.Rarity.Sport:
                return colorSport;
            case ItemData.Rarity.Elite:
                return colorElite;
            default:
                return colorBasic;
        }
    }
}

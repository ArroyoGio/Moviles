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

        statsText.text = stats;

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

        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(() =>
        {
            Debug.Log("CLICK " + item.itemName);
            callback?.Invoke(tipo, item);
        });
    }
}
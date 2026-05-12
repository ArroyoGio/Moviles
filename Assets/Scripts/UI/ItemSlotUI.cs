using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemSlotUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image itemIcon;
    public Image slotBackground;
    public TMP_Text slotLabel;
    public TMP_Text itemNameText;

    // Colores por rareza — igual que los colores de rol en CharacterCardUI
    private static readonly Color[] rarityColors = new Color[]
    {
        new Color(0.55f, 0.55f, 0.55f), // Basic  — gris
        new Color(0.0f,  0.47f, 1.0f),  // Sport  — azul
        new Color(0.85f, 0.65f, 0.0f),  // Elite  — dorado
    };

    private ItemData.ItemSlot tipoSlot;
    private Action<ItemData.ItemSlot, ItemData> onClickCallback;
    private ItemData itemActual;

    public void Setup(ItemData item, ItemData.ItemSlot tipo,
                      Action<ItemData.ItemSlot, ItemData> callback)
    {
        tipoSlot = tipo;
        onClickCallback = callback;
        itemActual = item;

        // Etiqueta del slot
        slotLabel.text = tipo switch
        {
            ItemData.ItemSlot.Weapon => "Arma",
            ItemData.ItemSlot.Protection => "Protección",
            ItemData.ItemSlot.Accessory => "Accesorio",
            ItemData.ItemSlot.Consumable => "Consumible",
            _ => tipo.ToString()
        };

        if (item != null)
        {
            // Ícono
            if (itemIcon != null && item.icon != null)
                itemIcon.sprite = item.icon;

            // Nombre
            if (itemNameText != null)
                itemNameText.text = item.itemName;

            // Color por rareza
            if (slotBackground != null)
                slotBackground.color = rarityColors[(int)item.rarity];
        }
        else
        {
            // Slot vacío
            if (itemNameText != null)
                itemNameText.text = "Vacío";
            if (slotBackground != null)
                slotBackground.color = new Color(0.2f, 0.2f, 0.2f);
        }
    }

    // Llama esto desde el Button de Unity (onClick en Inspector)
    public void OnClick()
    {
        onClickCallback?.Invoke(tipoSlot, itemActual);
    }
}
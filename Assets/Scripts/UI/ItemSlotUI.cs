using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemSlotUI : MonoBehaviour
{
    public Image itemIcon;
    public TMP_Text slotLabel;

    public void Setup(ItemData item, ItemData.ItemSlot type, Action<ItemData.ItemSlot, ItemData> callback)
    {
        slotLabel.text = type.ToString();
        if (item != null && itemIcon != null)
            itemIcon.sprite = item.icon;
    }
}
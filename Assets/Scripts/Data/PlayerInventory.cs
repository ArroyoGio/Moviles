using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "PackAPunch/PlayerInventory")]
public class PlayerInventory : ScriptableObject
{
    public List<ItemData> itemsDisponibles = new List<ItemData>();

    public void AgregarItem(ItemData item)
    {
        itemsDisponibles.Add(item);
    }

    public void RemoverItem(ItemData item)
    {
        itemsDisponibles.Remove(item);
    }

    public List<ItemData> GetItemsPorSlot(ItemData.ItemSlot slot)
    {
        return itemsDisponibles.FindAll(i => i.slot == slot);
    }
}
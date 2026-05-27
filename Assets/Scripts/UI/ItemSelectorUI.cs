using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ItemSelectorUI : MonoBehaviour
{
    public Transform contentParent;
    public GameObject itemSlotPrefab;
    public TMP_Text tituloText;
    public Button btnCerrar;
    public PlayerInventory playerInventory;

    private ItemData.ItemSlot slotActual;
    private VeteranData veteranoActual;
    private System.Action onEquipado;

    void Start()
    {
        btnCerrar.onClick.AddListener(Cerrar);
    }

    public void Abrir(ItemData.ItemSlot slot,
                      VeteranData veterano,
                      System.Action callback)
    {
        slotActual = slot;
        veteranoActual = veterano;
        onEquipado = callback;

        tituloText.text = "SELECCIONAR " + slot;

        CargarItems();
        gameObject.SetActive(true);
    }

    void CargarItems()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        var items = playerInventory.GetItemsPorSlot(slotActual);

        Debug.Log("Cantidad items: " + items.Count);

        foreach (var item in items)
        {
            GameObject go =
                Instantiate(itemSlotPrefab, contentParent);

            ItemSlotUI ui =
                go.GetComponent<ItemSlotUI>();

            if (ui == null)
            {
                Debug.LogError("El prefab no tiene ItemSlotUI");
                continue;
            }

            ui.Setup(item, slotActual,
                (tipo, itemSeleccionado) =>
                {
                    Equipar(itemSeleccionado);
                });
        }
    }

    void Equipar(ItemData item)
    {
        Debug.Log("EQUIPANDO: " + item.itemName);

        switch (slotActual)
        {
            case ItemData.ItemSlot.Weapon:
                veteranoActual.weaponSlot = item;
                break;

            case ItemData.ItemSlot.Protection:
                veteranoActual.protectionSlot = item;
                break;

            case ItemData.ItemSlot.Accessory:
                veteranoActual.accessorySlot = item;
                break;

            case ItemData.ItemSlot.Consumable:
                veteranoActual.consumableSlot = item;
                break;
        }

        Debug.Log("CALLBACK: " + onEquipado);

        onEquipado?.Invoke();
        Cerrar();
    }

    void Cerrar()
    {
        gameObject.SetActive(false);
    }
}
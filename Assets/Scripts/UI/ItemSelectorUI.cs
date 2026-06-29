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
        PrepararPopupVisual();
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
        transform.SetAsLastSibling();
        PrepararPopupVisual();
        gameObject.SetActive(true);
    }

    void PrepararPopupVisual()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.overrideSorting = true;
        canvas.sortingOrder = 500;

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        var rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.12f, 0.12f);
            rect.anchorMax = new Vector2(0.88f, 0.88f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        var image = GetComponent<Image>();
        if (image != null)
            image.color = new Color(0.05f, 0.06f, 0.08f, 0.96f);
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

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ItemInventoryUI : MonoBehaviour
{
    public PlayerInventory playerInventory;

    private RectTransform gridParent;
    private TMP_Text detailText;
    private ItemData.ItemSlot? currentFilter;
    private readonly List<Button> filterButtons = new List<Button>();

    void Start()
    {
        EnsureEventSystem();
        EnsureInventory();
        BuildUI();
        RefreshItems();
    }

    private void EnsureInventory()
    {
        if (playerInventory != null) return;

#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:PlayerInventory", new[] { "Assets/ScriptableObjects" });
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            playerInventory = AssetDatabase.LoadAssetAtPath<PlayerInventory>(path);
        }
#endif
    }

    private void BuildUI()
    {
        var canvasGo = new GameObject("ItemInventoryCanvas");
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        var root = CreatePanel("Root", canvasGo.transform, Vector2.zero, Vector2.zero, Vector2.one, new Color(0.05f, 0.06f, 0.08f, 1f));
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        var title = CreateText("Title", root, "OBJETOS", 44, TextAlignmentOptions.Center, Color.white);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -28), new Vector2(420, 64), new Vector2(0.5f, 1f));

        var backButton = CreateButton("BackButton", root, "← Volver", new Vector2(24, -24), new Vector2(0, 1), new Vector2(110, 36));
        var backText = backButton.GetComponentInChildren<TMP_Text>();
        if (backText != null) backText.fontSize = 17;
        backButton.onClick.AddListener(() => SceneManager.LoadScene("MainHub"));

        var filterPanel = CreatePanel("Filters", root, Vector2.zero, new Vector2(0, 1), new Vector2(1, 1), new Color(0f, 0f, 0f, 0f));
        SetRect(filterPanel, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -104), new Vector2(0, 48), new Vector2(0.5f, 1f));
        var filterLayout = filterPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
        filterLayout.padding = new RectOffset(260, 260, 0, 0);
        filterLayout.spacing = 12;
        filterLayout.childControlWidth = true;
        filterLayout.childControlHeight = true;
        filterLayout.childForceExpandWidth = true;
        filterLayout.childForceExpandHeight = true;

        AddFilterButton(filterPanel, "Todos", null);
        AddFilterButton(filterPanel, "Armas", ItemData.ItemSlot.Weapon);
        AddFilterButton(filterPanel, "Proteccion", ItemData.ItemSlot.Protection);
        AddFilterButton(filterPanel, "Accesorios", ItemData.ItemSlot.Accessory);
        AddFilterButton(filterPanel, "Consumibles", ItemData.ItemSlot.Consumable);

        var listPanel = CreatePanel("ItemsPanel", root, Vector2.zero, new Vector2(0, 0), new Vector2(0.68f, 1), new Color(0.08f, 0.09f, 0.12f, 1f));
        SetRect(listPanel, new Vector2(0, 0), new Vector2(0.68f, 1), new Vector2(24, -170), new Vector2(-16, -34), new Vector2(0, 1));

        var detailPanel = CreatePanel("DetailPanel", root, Vector2.zero, new Vector2(0.68f, 0), new Vector2(1, 1), new Color(0.09f, 0.1f, 0.13f, 1f));
        SetRect(detailPanel, new Vector2(0.68f, 0), new Vector2(1, 1), new Vector2(16, -170), new Vector2(-24, -34), new Vector2(0, 1));

        BuildItemGrid(listPanel);

        detailText = CreateText("DetailText", detailPanel, "Selecciona un objeto", 26, TextAlignmentOptions.TopLeft, Color.white);
        SetRect(detailText.rectTransform, Vector2.zero, Vector2.one, new Vector2(24, -24), new Vector2(-48, -48), new Vector2(0, 1));
        detailText.rectTransform.offsetMin = new Vector2(24, 24);
        detailText.rectTransform.offsetMax = new Vector2(-24, -24);
    }

    private void BuildItemGrid(RectTransform parent)
    {
        var scrollGo = new GameObject("ItemScroll");
        scrollGo.transform.SetParent(parent, false);
        var scrollRect = scrollGo.AddComponent<RectTransform>();
        SetRect(scrollRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
        scrollRect.offsetMin = new Vector2(18, 18);
        scrollRect.offsetMax = new Vector2(-18, -18);

        var scroll = scrollGo.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;

        var viewport = CreatePanel("Viewport", scrollRect, Vector2.zero, Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0f));
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = Vector2.zero;
        viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        scroll.viewport = viewport;

        var content = new GameObject("Content");
        content.transform.SetParent(viewport, false);
        gridParent = content.AddComponent<RectTransform>();
        SetRect(gridParent, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero, new Vector2(0.5f, 1));

        var grid = content.AddComponent<GridLayoutGroup>();
        grid.padding = new RectOffset(12, 12, 12, 12);
        grid.spacing = new Vector2(12, 12);
        grid.cellSize = new Vector2(250, 140);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;

        var fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = gridParent;
    }

    private void AddFilterButton(RectTransform parent, string label, ItemData.ItemSlot? filter)
    {
        var button = CreateButton("Filter" + label, parent, label, Vector2.zero, Vector2.zero, Vector2.zero);
        button.onClick.AddListener(() =>
        {
            currentFilter = filter;
            RefreshItems();
        });
        filterButtons.Add(button);
    }

    private void RefreshItems()
    {
        if (gridParent == null) return;

        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        if (playerInventory == null || playerInventory.itemsDisponibles == null || playerInventory.itemsDisponibles.Count == 0)
        {
            CreateEmptyCard("No hay objetos disponibles");
            return;
        }

        foreach (ItemData item in playerInventory.itemsDisponibles)
        {
            if (item == null) continue;
            if (currentFilter.HasValue && item.slot != currentFilter.Value) continue;
            CreateItemCard(item);
        }
    }

    private void CreateItemCard(ItemData item)
    {
        var button = CreateButton("ItemCard", gridParent, "", Vector2.zero, Vector2.zero, Vector2.zero);
        var image = button.GetComponent<Image>();
        if (image != null)
            image.color = GetCardColor(item.rarity);

        var text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.fontSize = 19;
            text.alignment = TextAlignmentOptions.Center;
            text.text = $"{item.itemName}\n{item.rarity} · {GetSlotLabel(item.slot)}\n{GetBonusSummary(item)}";
        }

        button.onClick.AddListener(() => ShowItemDetail(item));
    }

    private void CreateEmptyCard(string message)
    {
        var text = CreateText("EmptyInventory", gridParent, message, 24, TextAlignmentOptions.Center, Color.white);
        var layout = text.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = 520;
        layout.preferredHeight = 120;
    }

    private void ShowItemDetail(ItemData item)
    {
        if (detailText == null || item == null) return;

        detailText.text =
            $"{item.itemName}\n\n" +
            $"Rareza: {item.rarity}\n" +
            $"Tipo: {GetSlotLabel(item.slot)}\n\n" +
            $"{GetDescription(item)}\n\n" +
            $"Bonos:\n{GetBonusLines(item)}";
    }

    private string GetDescription(ItemData item)
    {
        if (!string.IsNullOrWhiteSpace(item.description))
            return item.description;

        if (item.slot == ItemData.ItemSlot.Consumable && !string.IsNullOrWhiteSpace(item.efectoTexto))
            return item.efectoTexto;

        return "Objeto disponible para equipamiento.";
    }

    private string GetBonusSummary(ItemData item)
    {
        string summary = GetBonusLines(item).Replace("\n", " · ");
        return string.IsNullOrEmpty(summary) ? "Sin bonus directo" : summary;
    }

    private string GetBonusLines(ItemData item)
    {
        var lines = new List<string>();
        if (item.bonusLife != 0) lines.Add($"+{item.bonusLife} HP");
        if (item.bonusDamage != 0) lines.Add($"+{item.bonusDamage} DMG");
        if (item.bonusAgility != 0) lines.Add($"+{item.bonusAgility} AGIL");
        if (item.bonusStamina != 0) lines.Add($"+{item.bonusStamina} STM");
        if (item.bonusDefense != 0) lines.Add($"+{item.bonusDefense * 100f:F0}% DEF");
        if (item.bonusEvasion != 0) lines.Add($"+{item.bonusEvasion * 100f:F0}% EVA");
        if (item.bonusCrit != 0) lines.Add($"+{item.bonusCrit * 100f:F0}% SRT");
        if (!string.IsNullOrWhiteSpace(item.eliteEffect)) lines.Add(item.eliteEffect);
        if (item.slot == ItemData.ItemSlot.Consumable && !string.IsNullOrWhiteSpace(item.condicion)) lines.Add(item.condicion);
        return string.Join("\n", lines);
    }

    private string GetSlotLabel(ItemData.ItemSlot slot)
    {
        switch (slot)
        {
            case ItemData.ItemSlot.Weapon: return "Arma";
            case ItemData.ItemSlot.Protection: return "Proteccion";
            case ItemData.ItemSlot.Accessory: return "Accesorio";
            case ItemData.ItemSlot.Consumable: return "Consumible";
            default: return slot.ToString();
        }
    }

    private Color GetCardColor(ItemData.Rarity rarity)
    {
        switch (rarity)
        {
            case ItemData.Rarity.Sport: return new Color(0.11f, 0.2f, 0.3f, 1f);
            case ItemData.Rarity.Elite: return new Color(0.22f, 0.16f, 0.08f, 1f);
            default: return new Color(0.13f, 0.15f, 0.18f, 1f);
        }
    }

    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();
    }

    private RectTransform CreatePanel(string name, Transform parent, Vector2 position, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        SetRect(rect, anchorMin, anchorMax, position, Vector2.zero, new Vector2(0.5f, 0.5f));
        var image = go.AddComponent<Image>();
        image.color = color;
        return rect;
    }

    private TMP_Text CreateText(string name, Transform parent, string text, int size, TextAlignmentOptions alignment, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.alignment = alignment;
        label.color = color;
        label.raycastTarget = false;
        return label;
    }

    private Button CreateButton(string name, Transform parent, string text, Vector2 position, Vector2 anchor, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        SetRect(rect, anchor, anchor, position, size, new Vector2(0.5f, 0.5f));

        var image = go.AddComponent<Image>();
        image.color = new Color(0.16f, 0.2f, 0.25f, 1f);

        var button = go.AddComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = new Color(0.24f, 0.32f, 0.42f);
        colors.pressedColor = new Color(0.08f, 0.12f, 0.18f);
        button.colors = colors;

        var label = CreateText("Text", rect, text, 22, TextAlignmentOptions.Center, Color.white);
        SetRect(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
        label.rectTransform.offsetMin = new Vector2(8, 6);
        label.rectTransform.offsetMax = new Vector2(-8, -6);
        return button;
    }

    private void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Vector2 pivot)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.pivot = pivot;
    }
}

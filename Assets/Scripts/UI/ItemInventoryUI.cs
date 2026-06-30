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
    private Image detailIcon;
    private TMP_Text counterText;
    private TMP_Text detailText;
    private ItemData.ItemSlot? currentFilter;
    private ItemData selectedItem;
    private Image selectedCardBackground;
    private Outline selectedCardOutline;
    private readonly List<ItemData> visibleItems = new List<ItemData>();

    void Start()
    {
        EnsureEventSystem();
        EnsureInventory();
        BuildUI();
        RefreshItems();
    }

    private void EnsureInventory()
    {
        if (playerInventory == null)
        {
#if UNITY_EDITOR
            string[] inventoryGuids = AssetDatabase.FindAssets("t:PlayerInventory", new[] { "Assets/ScriptableObjects" });
            if (inventoryGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(inventoryGuids[0]);
                playerInventory = AssetDatabase.LoadAssetAtPath<PlayerInventory>(path);
            }
#endif
        }

        PopulateInventoryWithAllKnownItems();
    }

    private void PopulateInventoryWithAllKnownItems()
    {
        if (playerInventory == null || playerInventory.itemsDisponibles == null) return;

#if UNITY_EDITOR
        string[] itemGuids = AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/ScriptableObjects/Items" });
        foreach (string guid in itemGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (item != null && !playerInventory.itemsDisponibles.Contains(item))
                playerInventory.itemsDisponibles.Add(item);
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

        counterText = CreateText("CounterText", root, "Objetos: 0", 24, TextAlignmentOptions.Right, new Color(0.82f, 0.86f, 0.9f));
        SetRect(counterText.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-32, -36), new Vector2(260, 36), new Vector2(1, 1));

        var backButton = CreateButton("BackButton", root, "< Volver", new Vector2(28, -28), new Vector2(0, 1), new Vector2(130, 44));
        var backText = backButton.GetComponentInChildren<TMP_Text>();
        if (backText != null) backText.fontSize = 18;
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
        BuildDetailPanel(detailPanel);
    }

    private void BuildDetailPanel(RectTransform detailPanel)
    {
        detailIcon = CreateItemIcon("DetailIcon", detailPanel, 118);
        SetRect(detailIcon.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(118, 118), new Vector2(0.5f, 1));

        detailText = CreateText("DetailText", detailPanel, "Selecciona un objeto", 16, TextAlignmentOptions.TopLeft, Color.white);
        SetRect(detailText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0, 1));
        detailText.rectTransform.offsetMin = new Vector2(24, 24);
        detailText.rectTransform.offsetMax = new Vector2(-24, -168);
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

        var viewport = CreatePanel("Viewport", scrollRect, Vector2.zero, Vector2.zero, Vector2.one, new Color(0.06f, 0.07f, 0.09f, 1f));
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = Vector2.zero;
        viewport.gameObject.AddComponent<RectMask2D>();
        scroll.viewport = viewport;

        var content = new GameObject("Content");
        content.transform.SetParent(viewport, false);
        gridParent = content.AddComponent<RectTransform>();
        SetRect(gridParent, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero, new Vector2(0, 1));
        gridParent.offsetMin = Vector2.zero;
        gridParent.offsetMax = Vector2.zero;

        var grid = content.AddComponent<GridLayoutGroup>();
        grid.padding = new RectOffset(16, 16, 16, 16);
        grid.spacing = new Vector2(36.1f, 37f);
        grid.cellSize = new Vector2(411f, 220.7f);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;

        var fitter = content.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
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
    }

    private void RefreshItems()
    {
        if (gridParent == null) return;

        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        visibleItems.Clear();

        if (playerInventory == null || playerInventory.itemsDisponibles == null || playerInventory.itemsDisponibles.Count == 0)
        {
            CreateEmptyCard("No hay objetos disponibles");
            UpdateCounter();
            return;
        }

        foreach (ItemData item in playerInventory.itemsDisponibles)
        {
            if (item == null) continue;
            if (currentFilter.HasValue && item.slot != currentFilter.Value) continue;

            visibleItems.Add(item);
            CreateItemCard(item);
        }

        if (visibleItems.Count == 0)
            CreateEmptyCard("No hay objetos en esta categoria");

        UpdateCounter();
    }

    private void CreateItemCard(ItemData item)
    {
        var button = CreateButton("ItemCard", gridParent, "", Vector2.zero, Vector2.zero, Vector2.zero);
        button.transform.SetAsLastSibling();

        var background = button.GetComponent<Image>();
        if (background != null)
            background.color = selectedItem == item ? GetSelectedCardColor() : GetCardColor(item.rarity);

        var outline = button.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.35f, 0.72f, 1f, 1f);
        outline.effectDistance = new Vector2(3, -3);
        outline.enabled = selectedItem == item;

        var rarityBar = CreatePanel("RarityBar", button.transform, Vector2.zero, new Vector2(0, 1), new Vector2(1, 1), GetRarityColor(item.rarity));
        SetRect(rarityBar, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, 6), new Vector2(0.5f, 1));
        rarityBar.offsetMin = new Vector2(0, -6);
        rarityBar.offsetMax = Vector2.zero;

        var icon = CreateItemIcon("Icon", button.transform, 62);
        SetRect(icon.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(46, 0), new Vector2(62, 62), new Vector2(0.5f, 0.5f));
        SetIconSprite(icon, item);

        var nameText = button.GetComponentInChildren<TMP_Text>();
        if (nameText != null)
        {
            nameText.fontSize = 19;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.textWrappingMode = TextWrappingModes.Normal;
            nameText.text = item.itemName;
            nameText.rectTransform.offsetMin = new Vector2(86, 44);
            nameText.rectTransform.offsetMax = new Vector2(-10, -16);
        }

        var rarityText = CreateText("RarityText", button.transform, item.rarity.ToString(), 15, TextAlignmentOptions.Left, GetRarityColor(item.rarity));
        SetRect(rarityText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
        rarityText.rectTransform.offsetMin = new Vector2(86, 18);
        rarityText.rectTransform.offsetMax = new Vector2(-10, -74);

        button.onClick.AddListener(() => SelectItem(item, background, outline));
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

        SetIconSprite(detailIcon, item);
        if (detailIcon != null)
            detailIcon.color = item.icon != null ? Color.white : GetRarityColor(item.rarity);

        detailText.text =
            $"<size=22><b>{item.itemName}</b></size>\n\n" +
            $"<size=15><color=#{ColorUtility.ToHtmlStringRGB(GetRarityColor(item.rarity))}>Rareza: {item.rarity}</color></size>\n" +
            $"<size=15>Tipo: {item.slot}</size>\n\n" +
            $"<size=16>{GetDescription(item)}</size>\n\n" +
            $"<size=16><b>Bonos:</b>\n{GetBonusLines(item)}</size>";
    }

    private void SelectItem(ItemData item, Image cardBackground, Outline cardOutline)
    {
        if (selectedCardBackground != null && selectedItem != null)
            selectedCardBackground.color = GetCardColor(selectedItem.rarity);

        if (selectedCardOutline != null)
            selectedCardOutline.enabled = false;

        selectedItem = item;
        selectedCardBackground = cardBackground;
        selectedCardOutline = cardOutline;

        if (selectedCardBackground != null)
            selectedCardBackground.color = GetSelectedCardColor();

        if (selectedCardOutline != null)
            selectedCardOutline.enabled = true;

        ShowItemDetail(item);
    }

    private void UpdateCounter()
    {
        if (counterText != null)
            counterText.text = $"Objetos: {visibleItems.Count}";
    }

    private string GetDescription(ItemData item)
    {
        if (!string.IsNullOrWhiteSpace(item.description))
            return item.description;

        if (item.slot == ItemData.ItemSlot.Consumable && !string.IsNullOrWhiteSpace(item.efectoTexto))
            return item.efectoTexto;

        return "Objeto disponible para equipamiento.";
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

        return lines.Count == 0 ? "Sin bonus directo" : string.Join("\n", lines);
    }

    private Color GetCardColor(ItemData.Rarity rarity)
    {
        switch (rarity)
        {
            case ItemData.Rarity.Sport: return new Color(0.12f, 0.22f, 0.32f, 1f);
            case ItemData.Rarity.Elite: return new Color(0.25f, 0.18f, 0.09f, 1f);
            default: return new Color(0.18f, 0.22f, 0.28f, 1f);
        }
    }

    private Color GetSelectedCardColor()
    {
        return new Color(0.2f, 0.34f, 0.48f, 1f);
    }

    private Color GetRarityColor(ItemData.Rarity rarity)
    {
        switch (rarity)
        {
            case ItemData.Rarity.Sport: return new Color(0.35f, 0.72f, 1f, 1f);
            case ItemData.Rarity.Elite: return new Color(1f, 0.72f, 0.24f, 1f);
            default: return new Color(0.78f, 0.82f, 0.88f, 1f);
        }
    }

    private Image CreateItemIcon(string name, Transform parent, float size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(size, size);
        var image = go.AddComponent<Image>();
        image.color = new Color(0.28f, 0.32f, 0.38f, 1f);
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private void SetIconSprite(Image image, ItemData item)
    {
        if (image == null || item == null) return;

        image.sprite = item.icon;
        image.preserveAspect = true;
        image.color = item.icon != null ? Color.white : GetRarityColor(item.rarity);
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

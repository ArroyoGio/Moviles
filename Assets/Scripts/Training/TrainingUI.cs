using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrainingUI : MonoBehaviour
{
    public TrainingManager manager;

    private TMP_Text headerText;
    private TMP_Text statsText;
    private TMP_Text messageText;
    private TMP_Text characterNameText;
    private RectTransform characterPlaceholder;
    private RectTransform feedbackLayer;
    private Coroutine characterFeedbackRoutine;
    private Transform veteranListParent;
    private readonly List<TrainingStatButton> statButtons = new List<TrainingStatButton>();

    void Start()
    {
        if (manager == null)
            manager = GetComponent<TrainingManager>();

        if (manager == null)
            manager = FindFirstObjectByType<TrainingManager>();

        manager.EnsureVeteransLoaded();
        BuildUI();

        manager.OnStateChanged += Refresh;
        manager.OnMessage += SetMessage;
        manager.OnTrainingCompleted += HandleTrainingCompleted;
        Refresh();
    }

    void OnDestroy()
    {
        if (manager == null) return;

        manager.OnStateChanged -= Refresh;
        manager.OnMessage -= SetMessage;
        manager.OnTrainingCompleted -= HandleTrainingCompleted;
    }

    private void BuildUI()
    {
        EnsureEventSystem();

        var canvasGo = new GameObject("TrainingCanvas");
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        var root = CreatePanel("Root", canvasGo.transform, Vector2.zero, new Vector2(0, 0), new Vector2(1, 1), new Color(0.05f, 0.06f, 0.08f, 1f));
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        headerText = CreateText("Header", root, "ENTRENAMIENTO", 42, TextAlignmentOptions.Center, Color.white);
        SetRect(headerText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -26), new Vector2(780, 64), new Vector2(0.5f, 1f));

        var listPanel = CreatePanel("VeteranList", root, new Vector2(28, -110), new Vector2(0, 0), new Vector2(0, 1), new Color(0.09f, 0.1f, 0.13f, 1f));
        SetRect(listPanel, new Vector2(0, 0), new Vector2(0, 1), new Vector2(28, -110), new Vector2(430, -150), new Vector2(0, 1));

        var statsPanel = CreatePanel("StatsPanel", root, new Vector2(486, -110), new Vector2(0, 0), new Vector2(1, 1), new Color(0.08f, 0.09f, 0.12f, 1f));
        SetRect(statsPanel, new Vector2(0, 0), new Vector2(1, 1), new Vector2(486, -110), new Vector2(-32, -150), new Vector2(0, 1));

        BuildVeteranList(listPanel);
        BuildTrainingPanel(statsPanel, root);

        var backButton = CreateButton("BackButton", root, "← Volver", new Vector2(24, -24), new Vector2(0, 1), new Vector2(90, 32));
        var backText = backButton.GetComponentInChildren<TMP_Text>();
        if (backText != null)
            backText.fontSize = 16;
        backButton.onClick.AddListener(() => SceneManager.LoadScene("MainHub"));
    }

    private void BuildVeteranList(RectTransform parent)
    {
        var title = CreateText("ListTitle", parent, "Veteranos", 26, TextAlignmentOptions.Center, Color.white);
        SetRect(title.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -12), new Vector2(0, 44), new Vector2(0.5f, 1));

        var content = new GameObject("VeteranButtons");
        content.transform.SetParent(parent, false);
        var contentRect = content.AddComponent<RectTransform>();
        SetRect(contentRect, new Vector2(0, 0), new Vector2(1, 1), new Vector2(12, 12), new Vector2(-24, -70), new Vector2(0.5f, 0.5f));
        contentRect.offsetMin = new Vector2(12, 12);
        contentRect.offsetMax = new Vector2(-12, -66);

        var layout = content.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.spacing = 8;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        veteranListParent = contentRect;

        if (manager.veterans.Count == 0)
        {
            var emptyText = CreateText("EmptyText", veteranListParent, "No hay veteranos", 22, TextAlignmentOptions.Center, Color.white);
            var emptyLayout = emptyText.gameObject.AddComponent<LayoutElement>();
            emptyLayout.preferredHeight = 54;
            emptyLayout.minHeight = 54;
            emptyLayout.flexibleWidth = 1;
            return;
        }

        foreach (VeteranData veteran in manager.veterans)
        {
            if (veteran == null || veteran.baseData == null) continue;

            var button = CreateButton("VeteranButton", veteranListParent, veteran.baseData.characterName, Vector2.zero, new Vector2(0, 1), new Vector2(0, 54));
            var buttonLayout = button.gameObject.AddComponent<LayoutElement>();
            buttonLayout.preferredHeight = 54;
            buttonLayout.minHeight = 54;
            buttonLayout.flexibleWidth = 1;
            button.onClick.AddListener(() => manager.SelectVeteran(veteran));
        }
    }

    private void BuildTrainingPanel(RectTransform parent, RectTransform root)
    {
        statsText = CreateText("StatsText", parent, "Selecciona un veterano", 26, TextAlignmentOptions.TopLeft, Color.white);
        SetRect(statsText.rectTransform, new Vector2(0, 1), new Vector2(0.48f, 1), new Vector2(24, -24), new Vector2(-12, 310), new Vector2(0, 1));

        characterPlaceholder = CreatePanel("SelectedVeteranPlaceholder", root, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.13f, 0.16f, 0.2f, 1f));
        SetRect(characterPlaceholder, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260, 220), new Vector2(0.5f, 0.5f));
        characterPlaceholder.SetAsLastSibling();

        characterNameText = CreateText("CharacterName", characterPlaceholder, "Selecciona", 30, TextAlignmentOptions.Center, Color.white);
        SetRect(characterNameText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
        characterNameText.rectTransform.offsetMin = Vector2.zero;
        characterNameText.rectTransform.offsetMax = Vector2.zero;

        var feedbackGo = new GameObject("TrainingFeedbackLayer");
        feedbackGo.transform.SetParent(root, false);
        feedbackLayer = feedbackGo.AddComponent<RectTransform>();
        SetRect(feedbackLayer, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
        feedbackLayer.offsetMin = Vector2.zero;
        feedbackLayer.offsetMax = Vector2.zero;

        messageText = CreateText("MessageText", parent, "", 30, TextAlignmentOptions.Center, new Color(1f, 0.88f, 0.35f));
        SetRect(messageText.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 24), new Vector2(0, 58), new Vector2(0.5f, 0));

        var gridGo = new GameObject("TrainingButtons");
        gridGo.transform.SetParent(parent, false);
        var gridRect = gridGo.AddComponent<RectTransform>();
        SetRect(gridRect, new Vector2(0.5f, 0.18f), new Vector2(1f, 0.92f), new Vector2(8, 0), new Vector2(-24, 0), new Vector2(0.5f, 0.5f));

        var grid = gridGo.AddComponent<GridLayoutGroup>();
        grid.padding = new RectOffset(10, 10, 10, 10);
        grid.spacing = new Vector2(12, 12);
        grid.cellSize = new Vector2(220, 78);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;

        foreach (TrainingStatType stat in System.Enum.GetValues(typeof(TrainingStatType)))
        {
            var button = CreateButton("Train" + stat, gridRect, "", Vector2.zero, Vector2.zero, new Vector2(220, 78));
            var statButton = button.gameObject.AddComponent<TrainingStatButton>();
            statButton.Setup(manager, stat);
            statButtons.Add(statButton);
        }
    }

    private void Refresh()
    {
        VeteranData veteran = manager.SelectedVeteran;
        headerText.text = $"ENTRENAMIENTO  Turnos {manager.TurnsUsed}/{TrainingManager.MaxTurns}  Energia {TrainingManager.MaxEnergy - manager.EnergyUsed}/{TrainingManager.MaxEnergy}";

        if (veteran == null)
        {
            statsText.text = "Selecciona un veterano";
            if (characterNameText != null)
                characterNameText.text = "Selecciona";
        }
        else
        {
            statsText.text =
                $"{veteran.baseData.characterName}\n\n" +
                $"HP: {veteran.life}\n" +
                $"DMG: {veteran.damage}\n" +
                $"DEF: {veteran.defense * 100f:F0}%\n" +
                $"AGIL: {veteran.agility}\n" +
                $"SRT: {veteran.luck * 100f:F0}%\n" +
                $"EVA: {veteran.evasion * 100f:F0}%\n" +
                $"STM: {veteran.stamina}\n" +
                $"CRIT: {veteran.critMultiplier:F2}x\n" +
                $"EMP: {veteran.push}";

            if (characterNameText != null)
                characterNameText.text = veteran.baseData.characterName;
        }

        foreach (TrainingStatButton statButton in statButtons)
            statButton.Refresh();
    }

    private void SetMessage(string message)
    {
        messageText.text = message;
        Refresh();
    }

    private void HandleTrainingCompleted(VeteranData veteran, TrainingStatType stat, float amount, int currentTurns, int currentEnergy)
    {
        Refresh();
        messageText.text = manager.IsComplete
            ? $"Entrenamiento completado - {manager.GetStatName(stat)} +{FormatTrainingAmount(stat, amount)}"
            : $"{manager.GetStatName(stat)} +{FormatTrainingAmount(stat, amount)}";
        PlayTrainingFeedback(stat, amount);
    }

    public void PlayTrainingFeedback(TrainingStatType stat, float amount)
    {
        string label = $"+{FormatTrainingAmount(stat, amount)} {GetShortStatLabel(stat)}";
        Color color = GetFeedbackColor(stat);
        StartCoroutine(PlayFloatingText(label, color));

        // Futuro: conectar animacion del personaje segun el stat entrenado.
        // Futuro: mostrar texto flotante con el bonus obtenido.
        // Futuro: reproducir sonido de entrenamiento/exito.
        // Futuro: disparar particulas o efectos visuales por categoria de stat.

        if (characterFeedbackRoutine != null)
            StopCoroutine(characterFeedbackRoutine);

        characterFeedbackRoutine = StartCoroutine(PlayCharacterPlaceholderFeedback(stat));
    }

    private IEnumerator PlayFloatingText(string label, Color color)
    {
        if (feedbackLayer == null || characterPlaceholder == null) yield break;

        feedbackLayer.SetAsLastSibling();

        var text = CreateText("TrainingFloatingText", feedbackLayer, label, 34, TextAlignmentOptions.Center, color);
        text.fontStyle = FontStyles.Bold;
        text.raycastTarget = false;
        var rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(420, 72);
        rect.anchoredPosition = new Vector2(0, -120);
        rect.SetAsLastSibling();

        var group = text.gameObject.AddComponent<CanvasGroup>();
        Vector2 start = rect.anchoredPosition;
        const float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rect.anchoredPosition = start + new Vector2(0, Mathf.Lerp(0, 44, t));
            group.alpha = 1f - t;
            yield return null;
        }

        Destroy(text.gameObject);
    }

    private IEnumerator PlayCharacterPlaceholderFeedback(TrainingStatType stat)
    {
        if (characterPlaceholder == null) yield break;

        Vector3 originalScale = Vector3.one;
        Vector2 originalPosition = characterPlaceholder.anchoredPosition;
        const float duration = 0.25f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float pulse = Mathf.Sin(t * Mathf.PI);
            characterPlaceholder.localScale = originalScale * (1f + 0.12f * pulse);

            if (stat == TrainingStatType.Damage || stat == TrainingStatType.Push || stat == TrainingStatType.CritMultiplier)
                characterPlaceholder.anchoredPosition = originalPosition + new Vector2(Mathf.Sin(t * Mathf.PI * 8f) * 10f, 0);
            else if (stat == TrainingStatType.Agility || stat == TrainingStatType.Evasion)
                characterPlaceholder.anchoredPosition = originalPosition + new Vector2(Mathf.Sin(t * Mathf.PI) * 24f, 0);

            yield return null;
        }

        characterPlaceholder.localScale = originalScale;
        characterPlaceholder.anchoredPosition = originalPosition;
        characterFeedbackRoutine = null;
    }

    private Color GetFeedbackColor(TrainingStatType stat)
    {
        switch (stat)
        {
            case TrainingStatType.Life:
            case TrainingStatType.Stamina:
                return new Color(0.35f, 1f, 0.48f);
            case TrainingStatType.Damage:
            case TrainingStatType.Push:
                return new Color(1f, 0.28f, 0.22f);
            case TrainingStatType.CritMultiplier:
            case TrainingStatType.Luck:
                return new Color(1f, 0.84f, 0.22f);
            case TrainingStatType.Defense:
                return new Color(0.28f, 0.56f, 1f);
            case TrainingStatType.Agility:
            case TrainingStatType.Evasion:
                return new Color(0.35f, 0.9f, 1f);
            default:
                return Color.white;
        }
    }

    private string GetShortStatLabel(TrainingStatType stat)
    {
        switch (stat)
        {
            case TrainingStatType.Life: return "HP";
            case TrainingStatType.Stamina: return "STM";
            case TrainingStatType.Evasion: return "EVA";
            case TrainingStatType.Agility: return "AGIL";
            case TrainingStatType.Defense: return "DEF";
            case TrainingStatType.Damage: return "DMG";
            case TrainingStatType.Luck: return "SRT";
            case TrainingStatType.CritMultiplier: return "CRIT";
            case TrainingStatType.Push: return "EMP";
            default: return stat.ToString().ToUpperInvariant();
        }
    }

    private string FormatTrainingAmount(TrainingStatType stat, float amount)
    {
        switch (stat)
        {
            case TrainingStatType.Defense:
            case TrainingStatType.Evasion:
            case TrainingStatType.Luck:
                return $"{amount * 100f:F0}%";
            case TrainingStatType.CritMultiplier:
                return $"{amount:F2}x";
            default:
                return Mathf.RoundToInt(amount).ToString();
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
        var rect = go.AddComponent<RectTransform>();
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
        image.color = new Color(0.16f, 0.22f, 0.3f, 1f);

        var button = go.AddComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = new Color(0.24f, 0.34f, 0.46f);
        colors.pressedColor = new Color(0.08f, 0.12f, 0.18f);
        button.colors = colors;

        var label = CreateText("Text", rect, text, 22, TextAlignmentOptions.Center, Color.white);
        SetRect(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;
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

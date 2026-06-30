using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CombatRewardsPanel : MonoBehaviour
{
    private GameObject panelRoot;
    private TMP_Text titleText;
    private TMP_Text winnerText;
    private TMP_Text rewardText;
    private bool rewardGranted;
    private AIBrain[] pausedBrains;

    void OnEnable()
    {
        ArenaManager.OnMatchEnded += HandleMatchEnded;
    }

    void OnDisable()
    {
        ArenaManager.OnMatchEnded -= HandleMatchEnded;
    }

    private async void HandleMatchEnded(CombatResult result)
    {
        if (result == null) return;

        PauseCombatActors();
        EnsurePanel();

        bool localWon = result.winner == 0;
        int rewardAmount = localWon ? 20 : 5;
        string winnerName = GetWinnerName(result.winner);

        titleText.text = localWon ? "VICTORIA" : "DERROTA";
        winnerText.text = "Ganador: " + winnerName;
        rewardText.text =
            "Recompensas\n\n" +
            $"+{rewardAmount} Fichas\n" +
            "+0 Fichas doradas";

        panelRoot.SetActive(true);
        panelRoot.transform.SetAsLastSibling();

        if (rewardGranted) return;
        rewardGranted = true;

        EconomyManager economy = EconomyManager.Instance;
        if (economy == null)
        {
            var economyGo = new GameObject("EconomyManager");
            economy = economyGo.AddComponent<EconomyManager>();
        }

        await economy.AddFichas(rewardAmount);
    }

    private string GetWinnerName(int winner)
    {
        if (winner == 0)
            return GetVeteranName(TeamManager.Instance?.equipoActual?.activos[0], "Jugador");

        if (winner == 1)
            return GetVeteranName(TeamManager.Instance?.equipoActual?.activos[1], "Rival");

        return "Empate";
    }

    private string GetVeteranName(VeteranData veteran, string fallback)
    {
        if (veteran != null && veteran.baseData != null && !string.IsNullOrEmpty(veteran.baseData.characterName))
            return veteran.baseData.characterName;

        return fallback;
    }

    private void EnsurePanel()
    {
        if (panelRoot != null) return;

        var canvasGo = new GameObject("CombatResultCanvas");
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 999;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();

        panelRoot = CreatePanel("CombatResultPanel", canvasGo.transform, new Color(0.04f, 0.05f, 0.07f, 0.94f)).gameObject;
        var panelRect = panelRoot.GetComponent<RectTransform>();
        SetRect(panelRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620, 470), new Vector2(0.5f, 0.5f));

        titleText = CreateText("Title", panelRect, "", 54, TextAlignmentOptions.Center, Color.white);
        SetRect(titleText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -34), new Vector2(0, 72), new Vector2(0.5f, 1));

        winnerText = CreateText("Winner", panelRect, "", 28, TextAlignmentOptions.Center, new Color(0.86f, 0.9f, 0.96f));
        SetRect(winnerText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -115), new Vector2(0, 44), new Vector2(0.5f, 1));

        rewardText = CreateText("Rewards", panelRect, "", 30, TextAlignmentOptions.Center, new Color(1f, 0.86f, 0.34f));
        SetRect(rewardText.rectTransform, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, -20), new Vector2(0, 160), new Vector2(0.5f, 0.5f));

        var rematchButton = CreateButton("RematchButton", panelRect, "Revancha", new Vector2(-145, 48), new Vector2(0.5f, 0), new Vector2(220, 58));
        rematchButton.transform.SetAsLastSibling();
        rematchButton.onClick.AddListener(Rematch);

        var hubButton = CreateButton("MainHubButton", panelRect, "Volver al MainHub", new Vector2(145, 48), new Vector2(0.5f, 0), new Vector2(250, 58));
        hubButton.transform.SetAsLastSibling();
        hubButton.onClick.AddListener(GoToMainHub);

        panelRoot.SetActive(false);
    }

    private void Rematch()
    {
        ResumeCombatActors();
        SceneManager.LoadScene("Combat");
    }

    private void GoToMainHub()
    {
        ResumeCombatActors();
        SceneManager.LoadScene("MainHub");
    }

    private void PauseCombatActors()
    {
        pausedBrains = FindObjectsByType<AIBrain>(FindObjectsSortMode.None);
        foreach (AIBrain brain in pausedBrains)
        {
            if (brain != null)
                brain.enabled = false;
        }
    }

    private void ResumeCombatActors()
    {
        if (pausedBrains == null) return;

        foreach (AIBrain brain in pausedBrains)
        {
            if (brain != null)
                brain.enabled = true;
        }

        pausedBrains = null;
    }

    private RectTransform CreatePanel(string name, Transform parent, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
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
        image.color = new Color(0.16f, 0.24f, 0.34f, 1f);

        var button = go.AddComponent<Button>();
        button.interactable = true;
        var colors = button.colors;
        colors.highlightedColor = new Color(0.26f, 0.38f, 0.52f);
        colors.pressedColor = new Color(0.08f, 0.14f, 0.22f);
        button.colors = colors;

        var label = CreateText("Text", rect, text, 24, TextAlignmentOptions.Center, Color.white);
        SetRect(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;

        return button;
    }

    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();
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

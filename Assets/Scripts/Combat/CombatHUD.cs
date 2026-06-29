using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatHUD : MonoBehaviour
{
    private Canvas canvas;
    private TMP_Text localNameText;
    private TMP_Text rivalNameText;
    private TMP_Text localHpText;
    private TMP_Text rivalHpText;
    private TMP_Text roundText;
    private TMP_Text centerText;
    private Image localHpFill;
    private Image rivalHpFill;
    private Fighter localFighter;
    private Fighter rivalFighter;
    private Fighter lastKnockedOut;
    private string localName = "Jugador";
    private string rivalName = "Rival";

    void OnEnable()
    {
        HealthSystem.OnHealthChanged += HandleHealthChanged;
        HealthSystem.OnDamageTaken += HandleDamageTaken;
        HealthSystem.OnKnockout += HandleKnockout;
        AttackSystem.OnAttackFeedback += HandleAttackFeedback;
        CombatSystem.OnFightersReady += HandleFightersReady;
        ArenaManager.OnRoundStarted += HandleRoundStarted;
        ArenaManager.OnBreakStarted += HandleBreakStarted;
        ArenaManager.OnRoundEnded += HandleRoundEnded;
        ArenaManager.OnMatchEnded += HandleMatchEnded;
    }

    void OnDisable()
    {
        HealthSystem.OnHealthChanged -= HandleHealthChanged;
        HealthSystem.OnDamageTaken -= HandleDamageTaken;
        HealthSystem.OnKnockout -= HandleKnockout;
        AttackSystem.OnAttackFeedback -= HandleAttackFeedback;
        CombatSystem.OnFightersReady -= HandleFightersReady;
        ArenaManager.OnRoundStarted -= HandleRoundStarted;
        ArenaManager.OnBreakStarted -= HandleBreakStarted;
        ArenaManager.OnRoundEnded -= HandleRoundEnded;
        ArenaManager.OnMatchEnded -= HandleMatchEnded;
    }

    public void Setup(VeteranData local, VeteranData rival)
    {
        localName = GetDisplayName(local, "Jugador");
        rivalName = GetDisplayName(rival, "Rival");

        EnsureHud();

        localNameText.text = localName;
        rivalNameText.text = rivalName;
        roundText.text = "ROUND 1";
        centerText.text = "";
        SetHp(1, local != null ? local.life : 0, local != null ? local.life : 0, 1f);
        SetHp(-1, rival != null ? rival.life : 0, rival != null ? rival.life : 0, 1f);
    }

    private string GetDisplayName(VeteranData veteran, string fallback)
    {
        if (veteran != null && veteran.baseData != null && !string.IsNullOrEmpty(veteran.baseData.characterName))
            return veteran.baseData.characterName;

        return fallback;
    }

    private void EnsureHud()
    {
        if (canvas != null) return;

        var canvasGo = new GameObject("CombatHUD");
        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        localNameText = CreateText("LocalName", canvas.transform, new Vector2(40, -32), TextAnchor.UpperLeft, 30, Color.white);
        localHpFill = CreateHealthBar("LocalHP", canvas.transform, new Vector2(40, -78), TextAnchor.UpperLeft, new Color(0.1f, 0.65f, 1f));
        localHpText = CreateText("LocalHPText", canvas.transform, new Vector2(40, -108), TextAnchor.UpperLeft, 22, Color.white);

        rivalNameText = CreateText("RivalName", canvas.transform, new Vector2(-40, -32), TextAnchor.UpperRight, 30, Color.white);
        rivalHpFill = CreateHealthBar("RivalHP", canvas.transform, new Vector2(-40, -78), TextAnchor.UpperRight, new Color(1f, 0.18f, 0.16f));
        rivalHpText = CreateText("RivalHPText", canvas.transform, new Vector2(-40, -108), TextAnchor.UpperRight, 22, Color.white);

        roundText = CreateText("RoundText", canvas.transform, new Vector2(0, -36), TextAnchor.UpperCenter, 38, new Color(1f, 0.9f, 0.45f));
        centerText = CreateText("CenterText", canvas.transform, Vector2.zero, TextAnchor.MiddleCenter, 64, Color.white);
    }

    private TMP_Text CreateText(string name, Transform parent, Vector2 anchoredPosition, TextAnchor anchor, int size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(520, 80);
        ApplyAnchor(rect, anchor);
        rect.anchoredPosition = anchoredPosition;

        var text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.color = color;
        text.alignment = ToTextAlignment(anchor);
        text.raycastTarget = false;
        return text;
    }

    private Image CreateHealthBar(string name, Transform parent, Vector2 anchoredPosition, TextAnchor anchor, Color fillColor)
    {
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);

        var rect = root.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(420, 24);
        ApplyAnchor(rect, anchor);
        rect.anchoredPosition = anchoredPosition;

        var bg = root.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.06f, 0.85f);
        bg.raycastTarget = false;

        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(root.transform, false);
        var fillRect = fillGo.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(3, 3);
        fillRect.offsetMax = new Vector2(-3, -3);

        var fill = fillGo.AddComponent<Image>();
        fill.color = fillColor;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 1f;
        fill.raycastTarget = false;

        return fill;
    }

    private void ApplyAnchor(RectTransform rect, TextAnchor anchor)
    {
        if (anchor == TextAnchor.UpperLeft)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
        }
        else if (anchor == TextAnchor.UpperRight)
        {
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
        }
        else if (anchor == TextAnchor.UpperCenter)
        {
            rect.anchorMin = new Vector2(0.5f, 1);
            rect.anchorMax = new Vector2(0.5f, 1);
            rect.pivot = new Vector2(0.5f, 1);
        }
        else
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    private TextAlignmentOptions ToTextAlignment(TextAnchor anchor)
    {
        if (anchor == TextAnchor.UpperLeft) return TextAlignmentOptions.TopLeft;
        if (anchor == TextAnchor.UpperRight) return TextAlignmentOptions.TopRight;
        if (anchor == TextAnchor.UpperCenter) return TextAlignmentOptions.Top;
        return TextAlignmentOptions.Center;
    }

    private void HandleFightersReady(Fighter local, Fighter rival)
    {
        localFighter = local;
        rivalFighter = rival;
        lastKnockedOut = null;

        if (local != null && local.data != null)
            localName = GetDisplayName(local.data, "Jugador");

        if (rival != null && rival.data != null)
            rivalName = GetDisplayName(rival.data, "Rival");

        EnsureHud();
        localNameText.text = localName;
        rivalNameText.text = rivalName;

        if (local != null && local.health != null)
            SetHp(1, local.health.CurrentHealth, local.health.MaxHealth, local.health.HealthPercent);

        if (rival != null && rival.health != null)
            SetHp(-1, rival.health.CurrentHealth, rival.health.MaxHealth, rival.health.HealthPercent);
    }

    private void HandleHealthChanged(Fighter fighter, int current, int max, float percent)
    {
        int side = GetVisualSide(fighter);
        if (side == 0) return;

        SetHp(side, current, max, percent);
    }

    private void SetHp(int side, int current, int max, float percent)
    {
        EnsureHud();
        var fill = side == 1 ? localHpFill : rivalHpFill;
        fill.fillAmount = Mathf.Clamp01(percent);

        var hpText = side == 1 ? localHpText : rivalHpText;
        if (hpText != null)
            hpText.text = $"HP: {Mathf.Max(0, current)} / {Mathf.Max(0, max)}";
    }

    private void HandleDamageTaken(Fighter fighter, int amount, Vector3 worldPosition)
    {
        if (GetVisualSide(fighter) == 0) return;

        EnsureHud();
        StartCoroutine(ShowDamage(amount, worldPosition));
    }

    private void HandleAttackFeedback(Fighter fighter, string message, Color color, int size)
    {
        if (GetVisualSide(fighter) == 0) return;

        EnsureHud();
        StartCoroutine(ShowWorldText(message, fighter.transform.position + Vector3.up * 0.9f, color, size, 0.9f));
    }

    private IEnumerator ShowDamage(int amount, Vector3 worldPosition)
    {
        var text = CreateText("DamageText", canvas.transform, Vector2.zero, TextAnchor.MiddleCenter, 36, new Color(1f, 0.82f, 0.22f));
        text.text = "-" + amount;

        var rect = text.GetComponent<RectTransform>();
        Vector3 screen = Camera.main != null
            ? Camera.main.WorldToScreenPoint(worldPosition + Vector3.up * 0.8f)
            : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        rect.position = screen;

        float elapsed = 0f;
        const float duration = 0.8f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rect.position += Vector3.up * (80f * Time.deltaTime);
            text.alpha = 1f - elapsed / duration;
            yield return null;
        }

        Destroy(text.gameObject);
    }

    private void HandleKnockout(Fighter fighter)
    {
        int side = GetVisualSide(fighter);
        if (side == 0) return;

        lastKnockedOut = fighter;
        EnsureHud();

        string koName = side == 1 ? localName : rivalName;
        StartCoroutine(ShowWorldText("KO - " + koName, fighter.transform.position + Vector3.up * 0.9f, new Color(1f, 0.25f, 0.2f), 42, 1.4f));
    }

    private void HandleRoundStarted(int round)
    {
        EnsureHud();
        roundText.text = "ROUND " + round;
        centerText.text = "";
    }

    private void HandleBreakStarted()
    {
        EnsureHud();
        centerText.text = "BREAK";
        centerText.color = Color.white;
    }

    private void HandleRoundEnded(int winner, int localScore, int rivalScore)
    {
        EnsureHud();

        if (winner == 0)
            centerText.text = $"ROUND WINNER: {localName}\n{localScore} - {rivalScore}";
        else if (winner == 1)
            centerText.text = $"ROUND WINNER: {rivalName}\n{localScore} - {rivalScore}";
        else
            centerText.text = $"ROUND DRAW\n{localScore} - {rivalScore}";

        centerText.color = new Color(1f, 0.9f, 0.45f);
    }

    private void HandleMatchEnded(CombatResult result)
    {
        EnsureHud();

        if (result.requiresOvertime)
        {
            centerText.text = $"EMPATE\n{result.finalScores[0]} - {result.finalScores[1]}";
        }
        else if (lastKnockedOut == localFighter)
        {
            centerText.text = $"GANADOR: {rivalName}\n{result.finalScores[0]} - {result.finalScores[1]}";
        }
        else if (lastKnockedOut == rivalFighter)
        {
            centerText.text = $"GANADOR: {localName}\n{result.finalScores[0]} - {result.finalScores[1]}";
        }
        else
        {
            centerText.text = $"GANADOR: {(result.winner == 0 ? localName : rivalName)}\n{result.finalScores[0]} - {result.finalScores[1]}";
        }

        centerText.color = new Color(1f, 0.9f, 0.45f);
    }

    private int GetVisualSide(Fighter fighter)
    {
        if (fighter == null) return 0;
        if (fighter == localFighter) return 1;
        if (fighter == rivalFighter) return -1;

        return fighter.side;
    }

    private IEnumerator ShowWorldText(string message, Vector3 worldPosition, Color color, int size, float duration)
    {
        var text = CreateText("WorldText", canvas.transform, Vector2.zero, TextAnchor.MiddleCenter, size, color);
        text.text = message;

        var rect = text.GetComponent<RectTransform>();
        Vector3 screen = Camera.main != null
            ? Camera.main.WorldToScreenPoint(worldPosition)
            : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        rect.position = screen;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rect.position += Vector3.up * (45f * Time.deltaTime);
            text.alpha = 1f - elapsed / duration;
            yield return null;
        }

        Destroy(text.gameObject);
    }
}

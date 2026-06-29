using System.Collections;
using UnityEngine;

[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(StaminaSystem))]
[RequireComponent(typeof(AttackSystem))]
[RequireComponent(typeof(MovementSystem))]
[RequireComponent(typeof(WallBounce))]
[RequireComponent(typeof(AIBrain))]
public class Fighter : MonoBehaviour
{
    public VeteranData data;
    public int side; // 1 = jugador, -1 = rival

    public HealthSystem health { get; private set; }
    public StaminaSystem stamina { get; private set; }
    public AttackSystem attack { get; private set; }
    public MovementSystem movement { get; private set; }
    public bool IsKO { get; private set; }

    private Coroutine attackStepRoutine;

    void Awake()
    {
        health = GetComponent<HealthSystem>();
        stamina = GetComponent<StaminaSystem>();
        attack = GetComponent<AttackSystem>();
        movement = GetComponent<MovementSystem>();
    }

    public void Initialize(VeteranData veteranData, int combatSide)
    {
        data = veteranData;
        side = combatSide;
        IsKO = false;

        health.Initialize(data.life);
        stamina.Initialize(data.stamina);
        attack.Initialize(data);
        movement.Initialize(data.agility, side);

        var brain = GetComponent<AIBrain>();
        brain.enabled = true;
        brain.Initialize(data.baseData.role);

        ConfigurePlaceholderVisual();
    }

    void ConfigurePlaceholderVisual()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = side == 1
                ? new Color(0.1f, 0.65f, 1f)
                : new Color(1f, 0.18f, 0.16f);
            spriteRenderer.sortingOrder = side == 1 ? 10 : 11;
        }

        transform.localScale = Vector3.one * 1.35f;
        movement.ClampToSide();
    }

    public void PlayAttackStep(Vector3 targetPosition)
    {
        if (IsKO) return;

        if (attackStepRoutine != null)
            StopCoroutine(attackStepRoutine);

        attackStepRoutine = StartCoroutine(AttackStepCo(targetPosition));
    }

    public void PlayHitReaction(Vector3 attackerPosition)
    {
        if (IsKO) return;

        float direction = transform.position.x >= attackerPosition.x ? 1f : -1f;
        movement.ApplyDisplacement(new Vector2(direction * 0.18f, 0f));
    }

    public void MarkKO()
    {
        if (IsKO) return;

        IsKO = true;

        var brain = GetComponent<AIBrain>();
        if (brain != null)
            brain.enabled = false;

        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = Color.gray;

        transform.localScale = Vector3.one * 1.05f;
    }

    private IEnumerator AttackStepCo(Vector3 targetPosition)
    {
        Vector3 start = transform.position;
        Vector3 direction = (targetPosition - transform.position).normalized;
        Vector3 forward = movement.ConstrainPosition(start + direction * 0.16f);

        float elapsed = 0f;
        const float duration = 0.08f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, forward, elapsed / duration);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(forward, start, elapsed / duration);
            yield return null;
        }

        movement.ClampToSide();
        attackStepRoutine = null;
    }
}

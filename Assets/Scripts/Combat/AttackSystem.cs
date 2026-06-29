using System;
using UnityEngine;

public class AttackSystem : MonoBehaviour
{
    public struct AttackResult
    {
        public int damage;
        public bool isCrit;
        public bool evaded;
    }

    public static event Action<Fighter, string, Color, int> OnAttackFeedback;

    private VeteranData stats;
    private float attackInterval;
    private float timer = 0f;
    public float attackRange;

    public void Initialize(VeteranData data)
    {
        stats = data;
        attackRange = data.baseData.attackRange;
        attackInterval = Mathf.Clamp(3f - data.agility / 500f, 1f, 2.5f);
        Debug.Log($"{data.baseData.characterName} - attackInterval: {attackInterval} - attackRange: {attackRange}");
    }

    public void TryAttack(Fighter target)
    {
        Fighter attacker = GetComponent<Fighter>();
        if (attacker == null || attacker.IsKO || target == null || target.IsKO || target.health.IsDead())
            return;

        timer -= Time.deltaTime;
        Debug.Log($"TryAttack - timer: {timer:F2} - target: {target?.name}");
        if (timer > 0) return;
        timer = attackInterval;

        GetComponent<StaminaSystem>().ConsumeStamina();

        AttackResult result = ResolveAttackDetailed(attacker, target);
        int damage = result.damage;
        Debug.Log($"Dano resuelto: {damage}");

        if (result.evaded)
        {
            OnAttackFeedback?.Invoke(target, "EVADIÓ", new Color(0.6f, 0.85f, 1f), 34);
            return;
        }

        if (damage > 0)
        {
            attacker.PlayAttackStep(target.transform.position);
            target.health.TakeDamage(damage, !result.isCrit);

            if (result.isCrit)
                OnAttackFeedback?.Invoke(target, "CRÍTICO -" + damage, new Color(1f, 0.45f, 0.12f), 38);

            target.PlayHitReaction(attacker.transform.position);
        }
    }

    public static int ResolveAttack(Fighter attacker, Fighter defender)
    {
        return ResolveAttackDetailed(attacker, defender).damage;
    }

    // Orden exacto GDD 4.7
    public static AttackResult ResolveAttackDetailed(Fighter attacker, Fighter defender)
    {
        VeteranData atkData = attacker.data;
        VeteranData defData = defender.data;

        // PASO 1 - Evasion
        if (UnityEngine.Random.value < defData.evasion)
            return new AttackResult { damage = 0, evaded = true };

        // PASO 2 - Critico
        bool isCrit = UnityEngine.Random.value < atkData.luck;
        int baseDmg = isCrit
            ? Mathf.RoundToInt(atkData.damage * atkData.critMultiplier)
            : atkData.damage;

        // PASO 3 - Empuje solo si critico
        if (isCrit)
        {
            Vector2 dir = (defender.transform.position - attacker.transform.position).normalized;
            defender.GetComponent<WallBounce>().ApplyPush(dir, atkData.push);
        }

        // PASO 4 - Defensa reduce el dano final
        int finalDmg = Mathf.RoundToInt(baseDmg * (1f - defData.defense));
        return new AttackResult
        {
            damage = Mathf.Max(1, finalDmg),
            isCrit = isCrit,
            evaded = false
        };
    }
}

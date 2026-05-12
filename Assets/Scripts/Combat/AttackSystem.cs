using UnityEngine;

public class AttackSystem : MonoBehaviour
{
    private VeteranData stats;
    private float attackInterval;
    private float timer = 0f;
    public float attackRange;

    public void Initialize(VeteranData data)
    {
        stats = data;
        attackRange = data.baseData.attackRange;
        attackInterval = Mathf.Clamp(3f - data.agility / 500f, 1f, 2.5f);
        Debug.Log($"{data.baseData.characterName} — attackInterval: {attackInterval} — attackRange: {attackRange}");
    }
    public void TryAttack(Fighter target)
    {
        timer -= Time.deltaTime;
        Debug.Log($"TryAttack — timer: {timer:F2} — target: {target?.name}");
        if (timer > 0) return;
        timer = attackInterval;

        GetComponent<StaminaSystem>().ConsumeStamina();

        Fighter attacker = GetComponent<Fighter>();
        int damage = ResolveAttack(attacker, target);
        Debug.Log($"Daño resuelto: {damage}");

        if (damage > 0)
            target.health.TakeDamage(damage);
    }
    // Orden exacto GDD 4.7
    public static int ResolveAttack(Fighter attacker, Fighter defender)
    {
        VeteranData atkData = attacker.data;
        VeteranData defData = defender.data;

        // PASO 1 — Evasión
        if (Random.value < defData.evasion)
            return 0;

        // PASO 2 — Crítico
        bool isCrit = Random.value < atkData.luck;
        int baseDmg = isCrit
            ? Mathf.RoundToInt(atkData.damage * atkData.critMultiplier)
            : atkData.damage;

        // PASO 3 — Empuje solo si crítico
        if (isCrit)
        {
            Vector2 dir = (defender.transform.position - attacker.transform.position).normalized;
            defender.GetComponent<WallBounce>().ApplyPush(dir, atkData.push);
        }

        // PASO 4 — Defensa reduce el daño final
        int finalDmg = Mathf.RoundToInt(baseDmg * (1f - defData.defense));
        return Mathf.Max(1, finalDmg);
    }
}
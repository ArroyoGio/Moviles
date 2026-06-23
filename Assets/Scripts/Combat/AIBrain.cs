using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIBrain : MonoBehaviour
{
    public enum State { Searching, Moving, Attacking, Recovering, Paused }

    private State currentState = State.Searching;
    private State lastState = State.Searching;
    private Fighter target;
    private RoleType role;
    private bool paused = false;

    public void Initialize(RoleType roleType) => role = roleType;

    void Update()
    {
        if (paused) return;
        CheckReactive();

        switch (currentState)
        {
            case State.Searching: Search(); break;
            case State.Moving: Move(); break;
            case State.Attacking: Attack(); break;
            case State.Recovering: Recover(); break;
        }

        // Log cuando cambia el estado para evitar spam cada frame
        if (currentState != lastState)
        {
            Debug.Log($"AIBrain estado cambiado — {lastState} -> {currentState} — CombatSystem: {CombatSystem.Instance}");
            lastState = currentState;
        }
    }

    void Search()
    {
        target = GetTargetByRole();

        // Solo cambia estado si el target es válido
        if (target != null && target.health != null && !target.health.IsDead())
            currentState = State.Moving;
        else
            target = null;
    }

    void Move()
    {
        // Verificación segura — comprueba null antes de acceder a health
        if (target == null)
        {
            currentState = State.Searching;
            return;
        }

        if (target.health == null || target.health.IsDead())
        {
            currentState = State.Searching;
            return;
        }

        float dist = Vector2.Distance(transform.position, target.transform.position);

        if (dist <= GetComponent<AttackSystem>().attackRange)
            currentState = State.Attacking;
        else
            GetComponent<MovementSystem>().MoveTowards(target.transform.position);
    }
    void Attack()
    {
        if (target == null || target.health == null || target.health.IsDead())
        {
            currentState = State.Searching;
            return;
        }

        // Si el rival se alejó, vuelve a moverse
        float dist = Vector2.Distance(transform.position, target.transform.position);
        if (dist > GetComponent<AttackSystem>().attackRange + 0.5f)
        {
            currentState = State.Moving;
            return;
        }

        var stamina = GetComponent<StaminaSystem>();
        bool exception = target.health.HealthPercent < 0.15f;

        if (!stamina.CanAttack() && !exception)
        {
            currentState = State.Recovering;
            return;
        }

        if (exception) stamina.ForceAttack();
        GetComponent<AttackSystem>().TryAttack(target);
    }

    void Recover()
    {
        GetComponent<MovementSystem>().Retreat();
        if (GetComponent<StaminaSystem>().CanAttack())
            currentState = State.Searching;
    }

    // GDD 4.5 — comportamientos reactivos
    void CheckReactive()
    {
        float hp = GetComponent<HealthSystem>().HealthPercent;
        GetComponent<MovementSystem>().SetDefensive(hp < 0.30f);

        if (hp < 0.15f)
            GetComponent<StaminaSystem>().ForceAttack();
    }

    Fighter GetTargetByRole()
    {
        if (CombatSystem.Instance == null) return null;

        List<Fighter> rivals = CombatSystem.Instance.GetActiveRivals(
            GetComponent<Fighter>().side);

        if (rivals == null || rivals.Count == 0) return null;

        // Fase 1 — todos apuntan al rival con menos vida
        Fighter best = null;
        float lowestHP = float.MaxValue;
        foreach (var r in rivals)
            if (r.health.HealthPercent < lowestHP)
            {
                lowestHP = r.health.HealthPercent;
                best = r;
            }
        return best;
    }

    public void Pause(float duration) => StartCoroutine(PauseCo(duration));

    IEnumerator PauseCo(float d)
    {
        paused = true;
        yield return new WaitForSeconds(d);
        paused = false;
    }
}
using UnityEngine;
using System;

public class StaminaSystem : MonoBehaviour
{
    private float currentStamina;
    private float maxStamina;

    private const float COST_PER_ATTACK = 0.15f;
    private const float RECOVERY_PER_SEC = 0.06f;
    private const float RETREAT_THRESHOLD = 0.25f;

    public float StaminaPercent => currentStamina / maxStamina;
    public bool CanAttack() => currentStamina > maxStamina * RETREAT_THRESHOLD;

    public static event Action<int, float> OnStaminaChanged;

    public void Initialize(int stamina)
    {
        maxStamina = stamina;
        currentStamina = stamina;
    }

    public void ConsumeStamina()
    {
        currentStamina = Mathf.Max(0, currentStamina - maxStamina * COST_PER_ATTACK);
        var fighter = GetComponent<Fighter>();
        OnStaminaChanged?.Invoke(fighter.side, StaminaPercent);
    }

    public void RestoreFull()
    {
        currentStamina = maxStamina;
        var fighter = GetComponent<Fighter>();
        OnStaminaChanged?.Invoke(fighter.side, 1f);
    }

    public void ForceAttack()
    {
        currentStamina = maxStamina * (RETREAT_THRESHOLD + 0.01f);
    }

    void Update()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(maxStamina,
                currentStamina + maxStamina * RECOVERY_PER_SEC * Time.deltaTime);
            var fighter = GetComponent<Fighter>();
            OnStaminaChanged?.Invoke(fighter.side, StaminaPercent);
        }
    }
}
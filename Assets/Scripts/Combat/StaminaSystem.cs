using UnityEngine;
using System;

public class StaminaSystem : MonoBehaviour
{
    private float currentStamina;
    private float maxStamina;

    // Valores exactos del GDD sección 4.8
    private const float COST_PER_ATTACK = 0.15f; // 15% por ataque
    private const float RECOVERY_PER_SEC = 0.06f; // 6% por segundo
    private const float RETREAT_THRESHOLD = 0.25f; // retrocede bajo 25%

    public float StaminaPercent => currentStamina / maxStamina;
    public bool CanAttack() => currentStamina > maxStamina * RETREAT_THRESHOLD;

    public static event Action<int, float> OnStaminaChanged; // (side, percent)

    public void Initialize(int stamina)
    {
        maxStamina = stamina;
        currentStamina = stamina;
    }

    public void ConsumeStamina()
    {
        currentStamina = Mathf.Max(0, currentStamina - maxStamina * COST_PER_ATTACK);
        OnStaminaChanged?.Invoke(GetComponent<Fighter>().side, StaminaPercent);
    }

    public void RestoreFull()
    {
        currentStamina = maxStamina;
        OnStaminaChanged?.Invoke(GetComponent<Fighter>().side, 1f);
    }

    // Excepción GDD 4.8 — si rival < 15% vida, ignora umbral
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
            OnStaminaChanged?.Invoke(GetComponent<Fighter>().side, StaminaPercent);
        }
    }
}
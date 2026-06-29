using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    private int currentHealth;
    private int maxHealth;

    public float HealthPercent => (float)currentHealth / maxHealth;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead() => currentHealth <= 0;

    public static event Action<Fighter, int, int, float> OnHealthChanged;
    public static event Action<Fighter, int, Vector3> OnDamageTaken;
    public static event Action<Fighter> OnKnockout;

    public void Initialize(int health)
    {
        maxHealth = health;
        currentHealth = health;

        var fighter = GetComponent<Fighter>();
        if (fighter != null)
            OnHealthChanged?.Invoke(fighter, currentHealth, maxHealth, HealthPercent);
    }

    public void TakeDamage(int amount, bool showFloatingText = true)
    {
        if (IsDead()) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        Debug.Log($"Dano recibido: {amount} - Vida actual: {currentHealth}/{maxHealth}");

        var fighter = GetComponent<Fighter>();
        OnHealthChanged?.Invoke(fighter, currentHealth, maxHealth, HealthPercent);
        if (showFloatingText)
            OnDamageTaken?.Invoke(fighter, amount, transform.position);

        if (currentHealth <= 0)
        {
            Debug.Log("KO notificado");
            fighter.MarkKO();
            OnKnockout?.Invoke(fighter);
            CombatSystem.Instance.NotifyKO(fighter);
        }
    }
}

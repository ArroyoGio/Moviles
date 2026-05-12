using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    private int currentHealth;
    private int maxHealth;

    public float HealthPercent => (float)currentHealth / maxHealth;
    public bool IsDead() => currentHealth <= 0;

    public static event Action<int, float> OnHealthChanged; // (side, percent)

    public void Initialize(int health)
    {
        maxHealth = health;
        currentHealth = health;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead()) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        Debug.Log($"Daño recibido: {amount} — Vida actual: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(GetComponent<Fighter>().side, HealthPercent);

        if (currentHealth <= 0)
        {
            Debug.Log("KO notificado");
            CombatSystem.Instance.NotifyKO(GetComponent<Fighter>());
        }
    }
}
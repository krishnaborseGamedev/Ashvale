using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        if (HealthBarUI.Instance != null) HealthBarUI.Instance.SetHealth(currentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log("Player took " + amount + " damage. Current health: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
        if (HealthBarUI.Instance != null) HealthBarUI.Instance.SetHealth(currentHealth, maxHealth);
    }

    void Die()
    {
        Debug.Log("Player has died.");
        if (GameEndUI.Instance != null) GameEndUI.Instance.ShowLose();
        // TODO (Phase 3/4): trigger lose screen, disable player input, etc.
    }
}
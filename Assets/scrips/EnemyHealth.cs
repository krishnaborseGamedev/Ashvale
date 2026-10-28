using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("Hit Feedback")]
    public GameObject hitEffectPrefab; // drag your Particle System prefab here

    private bool isDead = false;
    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();

    }

    public void TakeDamage(float amount, Vector3 hitPoint)
    {
        if (SfxManager.Instance != null) SfxManager.Instance.PlayHit(transform.position);
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log(gameObject.name + " took " + amount + " damage. Current health: " + currentHealth);

        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);
            Destroy(effect, 2f); // safety cleanup even if the particle's own Stop Action somehow fails
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

 void Die()
{
    isDead = true;
    Debug.Log(gameObject.name + " has died.");

    EnemyController enemyController = GetComponent<EnemyController>();
    if (enemyController != null)
    {
        enemyController.enabled = false;
    }

    UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    if (agent != null)
    {
        agent.enabled = false;
    }

    animator.SetBool("Die", true);

    Destroy(gameObject, 5f);
}
}
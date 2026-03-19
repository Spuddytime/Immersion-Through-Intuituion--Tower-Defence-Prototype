using UnityEngine;

// Handles enemy health and death
public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public EnemyUnitType unitType = EnemyUnitType.Ground;
    public int moneyReward = 5;

    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log(gameObject.name + " took damage. Current Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddMoney(moneyReward);
        }

        Debug.Log(gameObject.name + " died.");
        Destroy(gameObject);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
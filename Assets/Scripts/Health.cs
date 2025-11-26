using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    private PlayerController playerController;

    void Start()
    {
        currentHealth = maxHealth;
        playerController = GetComponent<PlayerController>();
    }

    public void TakeDamage(int damage)
    {
       
        if (playerController != null && playerController.isBlocking)
        {
            Debug.Log($"{name} The damage has been blocked!");
            return;
        }

        currentHealth -= damage;
        Debug.Log($"{name} got {damage} damage. HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{name} Died!");
        Destroy(gameObject);
    }
}
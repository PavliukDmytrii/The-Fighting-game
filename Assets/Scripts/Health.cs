using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    private int _currentHealth;
    private PlayerController playerController;

    [HideInInspector] public UnityEvent<int> onHealthChanged;

    public int CurrentHealth
    {
        get { return _currentHealth; }
        set
        {
            _currentHealth = Mathf.Clamp(value, 0, maxHealth);
            onHealthChanged.Invoke(_currentHealth);
            if(_currentHealth <= 0)
            {
                Die();
            }
        }
    }

    void Start()
    {
        CurrentHealth = maxHealth;
        playerController = GetComponent<PlayerController>();
    }

public void TakeDamage(int damage)
{
    if (playerController != null && playerController.isBlocking)
    {
        Debug.Log($"{name} The damage has been blocked!");
        return;
    }

    CurrentHealth -= damage;
    Debug.Log($"{name} got {damage} damage. HP: {CurrentHealth}");

    if (playerController != null && CurrentHealth > 0)
    {
        playerController.TakeHit();
    }

    if (CurrentHealth <= 0)
    {
        Die();
    }
}

void Die()
    {
        if (!this.enabled) return;

        Debug.Log($"{name} Died!");

        if (playerController != null)
        {
            playerController.LoseGame();
        }

        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController p in allPlayers)
        {
            if (p != playerController)
            {
                p.WinGame();
            }
        }

        this.enabled = false;
        
        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider != null) myCollider.enabled = false;
    }
}
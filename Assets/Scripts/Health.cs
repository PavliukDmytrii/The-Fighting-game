using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    private int _currentHealth;
    public PlayerController playerController;

    [HideInInspector] public UnityEvent<int> onHealthChanged;

    public int CurrentHealth
    {
        get { return _currentHealth; }
        set
        {
            _currentHealth = Mathf.Clamp(value, 0, maxHealth);
            onHealthChanged.Invoke(_currentHealth);
            if (_currentHealth <= 0)
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

        RoundTimer roundTimer = Object.FindFirstObjectByType<RoundTimer>();
        if (roundTimer != null)
        {
            if (gameObject.name == "Player1")
                roundTimer.lastHitter = 2;
            else
                roundTimer.lastHitter = 1;
        }

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
        this.enabled = false;

        //
        RoundTimer timer = FindFirstObjectByType<RoundTimer>();

        if (timer != null)
        {
            if (gameObject.name == "Player_1")
                timer.RoundOver(2);
            else
                timer.RoundOver(1);
        }
    }
}
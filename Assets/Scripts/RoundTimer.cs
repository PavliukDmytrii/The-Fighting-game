using UnityEngine;
using UnityEngine.UI;

public class RoundTimer : MonoBehaviour
{
    [Header("UI References")]
    public Sprite[] numberSprites; // 0-9 images
    public Image tensImage;
    public Image onesImage;

    [Header("Settings")]
    public float maxTime = 90f;

    [HideInInspector] public int lastHitter = 0; // 0 = none, 1 = player1, 2 = player2

    [Header("Players")]
    public Health player1Health;
    public Health player2Health;

    private float currentTime;
    private bool isRunning = true;

    void Start()
    {
        currentTime = maxTime;
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            isRunning = false;
            TimeUp();
        }

        UpdateTimerDisplay();
    }

    void UpdateTimerDisplay()
    {
        int time = Mathf.CeilToInt(currentTime);

        // Prevent index out of range
        if (time < 0) time = 0;
        if (time > 99) time = 99;

        int tens = time / 10;
        int ones = time % 10;

        tensImage.sprite = numberSprites[tens];
        onesImage.sprite = numberSprites[ones];
    }

    void TimeUp()
    {
        Debug.Log("TIME OVER");


        if (player1Health.CurrentHealth > player2Health.CurrentHealth)
        {
            Win(1);
        }
        else if (player2Health.CurrentHealth > player1Health.CurrentHealth)
        {
            Win(2);
        }
        else
        {
         
            if (lastHitter == 1) Win(1);
            else if (lastHitter == 2) Win(2);
            else Win(1); 
        }

        void Win(int playerIndex)
        {
            Debug.Log($"Player {playerIndex} Wins by Time Over!");

         
            if (playerIndex == 1)
            {
                player1Health.playerController.WinGame();
                player2Health.playerController.LoseGame();
            }
            else
            {
                player2Health.playerController.WinGame();
                player1Health.playerController.LoseGame();
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro; // Required for the Score Text

public class RoundTimer : MonoBehaviour
{
    [Header("UI References")]
    public Sprite[] numberSprites; // 0-9 images
    public Image tensImage;
    public Image onesImage;

    [Header("Score UI")]
    public TextMeshProUGUI p1ScoreText; // Assign your Player 1 Score Text here
    public TextMeshProUGUI p2ScoreText; // Assign your Player 2 Score Text here

    [Header("Intro Reference")]
    public FightIntro fightIntro;

    [Header("Round Settings")]
    public int roundsToWin = 2; // 2 out of 3
    private int p1Wins = 0;
    private int p2Wins = 0;
    private int currentRound = 1;

    [Header("Settings")]
    public float maxTime = 90f;

    [HideInInspector]
    public int lastHitter = 0; // 0 = none, 1 = player1, 2 = player2

    [Header("Players")]
    public Health player1Health;
    public Health player2Health;

    // Place to respawn players after round ends
    public Transform p1StartPoint;
    public Transform p2StartPoint;

    private float currentTime;
    private bool isRunning = false;

    void Start()
    {
        currentTime = maxTime;
        UpdateScoreUI(); // Initialize scores to 0 at start
        StartCoroutine(StartFirstRoundDelay());
    }

    IEnumerator StartFirstRoundDelay()
    {
        yield return null;
        if (fightIntro != null)
        {
            fightIntro.PlayIntroSequence(currentRound);
            isRunning = true;
        }
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

        if (tensImage != null) tensImage.sprite = numberSprites[tens];
        if (onesImage != null) onesImage.sprite = numberSprites[ones];
    }

    void UpdateScoreUI()
    {
        if (p1ScoreText != null) p1ScoreText.text = p1Wins.ToString();
        if (p2ScoreText != null) p2ScoreText.text = p2Wins.ToString();
    }

    void TimeUp()
    {
        Debug.Log("TIME OVER");

        if (player1Health.CurrentHealth > player2Health.CurrentHealth)
        {
            RoundOver(1);
        }
        else if (player2Health.CurrentHealth > player1Health.CurrentHealth)
        {
            RoundOver(2);
        }
        else
        {
            if (lastHitter == 1) RoundOver(1);
            else if (lastHitter == 2) RoundOver(2);
            else RoundOver(1);
        }
    }

    public void RoundOver(int winnerIndex)
    {
        // Safety check to prevent multiple triggers
        if (!isRunning && currentTime > 0) return;

        isRunning = false;

        Debug.Log($"Round Over! Winner: Player {winnerIndex}");

        // Increment wins
        if (winnerIndex == 1) p1Wins++;
        else if (winnerIndex == 2) p2Wins++;

        // Update the UI immediately
        UpdateScoreUI();

        // Trigger animations
        if (winnerIndex == 1)
        {
            player1Health.playerController.WinGame();
            player2Health.playerController.LoseGame();
        }
        else
        {
            player2Health.playerController.WinGame();
            player1Health.playerController.LoseGame();
        }

        StartCoroutine(NextRoundRoutine());
    }

    IEnumerator NextRoundRoutine()
    {
        yield return new WaitForSeconds(5f);

        if (p1Wins >= roundsToWin || p2Wins >= roundsToWin)
        {
            Debug.Log("GAME OVER! MATCH FINISHED");
            // You could load a Victory Scene here
        }
        else
        {
            ResetRound();
        }
    }

    void ResetRound()
    {
        Debug.Log("Starting Next Round...");
        currentRound++;

        currentTime = maxTime;
        isRunning = true;

        // Reset positions
        player1Health.transform.position = p1StartPoint.position;
        player2Health.transform.position = p2StartPoint.position;

        // Reset health and enable scripts
        player1Health.CurrentHealth = player1Health.maxHealth;
        player2Health.CurrentHealth = player2Health.maxHealth;

        player1Health.enabled = true;
        player2Health.enabled = true;

        // Reset animations
        player1Health.playerController.ResetState();
        player2Health.playerController.ResetState();

        if (fightIntro != null)
        {
            fightIntro.PlayIntroSequence(currentRound);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement; // Required for scene loading

public class RoundTimer : MonoBehaviour
{
    [Header("UI References")]
    public Sprite[] numberSprites;
    public Image tensImage;
    public Image onesImage;

    [Header("End Game Photos")]
    public GameObject timeOverPhoto;
    public GameObject gameOverPhoto;
    public GameObject continueButton;

    [Header("Score UI")]
    public TextMeshProUGUI p1ScoreText;
    public TextMeshProUGUI p2ScoreText;

    [Header("Intro Reference")]
    public FightIntro fightIntro;

    [Header("Round Settings")]
    public int roundsToWin = 2;
    private int p1Wins = 0;
    private int p2Wins = 0;
    private int currentRound = 1;

    [Header("Settings")]
    public float maxTime = 90f;

    [HideInInspector]
    public int lastHitter = 0;

    [Header("Players")]
    public Health player1Health;
    public Health player2Health;

    public Transform p1StartPoint;
    public Transform p2StartPoint;

    private float currentTime;
    private bool isRunning = false;

    void Start()
    {
        // Ensure photos are hidden at start
        if (timeOverPhoto) timeOverPhoto.SetActive(false);
        if (gameOverPhoto) gameOverPhoto.SetActive(false);
        if (continueButton) continueButton.SetActive(false);

        currentTime = maxTime;
        UpdateScoreUI();
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
        // Show Time Over photo immediately
        if (timeOverPhoto != null) timeOverPhoto.SetActive(true);

        if (player1Health.CurrentHealth > player2Health.CurrentHealth)
            RoundOver(1);
        else if (player2Health.CurrentHealth > player1Health.CurrentHealth)
            RoundOver(2);
        else
            RoundOver(lastHitter == 2 ? 2 : 1);
    }

    public void RoundOver(int winnerIndex)
    {
        if (!isRunning && currentTime > 0) return;

        isRunning = false;

        if (winnerIndex == 1) p1Wins++;
        else if (winnerIndex == 2) p2Wins++;

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

        // Check for Match Win immediately
        if (p1Wins >= roundsToWin || p2Wins >= roundsToWin)
        {
            if (gameOverPhoto != null) gameOverPhoto.SetActive(true);
            StartCoroutine(ShowContinueDelay());
        }
        else
        {
            StartCoroutine(NextRoundRoutine());
        }
    }

    IEnumerator ShowContinueDelay()
    {
        yield return new WaitForSeconds(3f); // Wait with Game Over on screen
        if (gameOverPhoto != null) gameOverPhoto.SetActive(false); // Hide Game Over
        if (continueButton != null) continueButton.SetActive(true); // Show Continue Button
    }

    IEnumerator NextRoundRoutine()
    {
        yield return new WaitForSeconds(5f);
        if (timeOverPhoto != null) timeOverPhoto.SetActive(false);
        ResetRound();
    }

    void ResetRound()
    {
        currentRound++;
        currentTime = maxTime;
        isRunning = true;

        player1Health.transform.position = p1StartPoint.position;
        player2Health.transform.position = p2StartPoint.position;
        player1Health.CurrentHealth = player1Health.maxHealth;
        player2Health.CurrentHealth = player2Health.maxHealth;
        player1Health.enabled = true;
        player2Health.enabled = true;
        player1Health.playerController.ResetState();
        player2Health.playerController.ResetState();

        if (fightIntro != null)
            fightIntro.PlayIntroSequence(currentRound);
    }

    // Button function to return to main menu
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
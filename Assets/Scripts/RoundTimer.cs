using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RoundTimer : MonoBehaviour
{
    [Header("UI References")]
    public Sprite[] numberSprites; // 0-9 images
    public Image tensImage;
    public Image onesImage;

    [Header("Intro Reference")]
    public FightIntro fightIntro;

    [Header("Round Settings")]
    public int roundsToWin = 2; //2 out of 3
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


    //place to respawn players after round ends
    public Transform p1StartPoint;
    public Transform p2StartPoint;
    private float currentTime;
    private bool isRunning = false;

    IEnumerator StartFirstRoundDelay()
    {
        yield return null;
        if (fightIntro != null)
        {
            fightIntro.PlayIntroSequence(currentRound);
            isRunning = true;

        }
    }

    void Start()
    {
        currentTime = maxTime;
        StartCoroutine(StartFirstRoundDelay());
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
            RoundOver(1);
        }
        else if (player2Health.CurrentHealth > player1Health.CurrentHealth)
        {
            RoundOver(1);
        }
        else
        {

            if (lastHitter == 1) RoundOver(1);
            else if (lastHitter == 2) RoundOver(2);
            else RoundOver(1);
        }
    }
    void Win(int playerIndex)
    {
        RoundOver(playerIndex);
    }


    // wins method
    public void RoundOver(int winnerIndex)
    {
        if (!isRunning) return; 
        isRunning = false;

        Debug.Log($"Round Over! Winner: Player {winnerIndex}");

        //
        if (winnerIndex == 1) p1Wins++;
        else if (winnerIndex == 2) p2Wins++;

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

    //Coroutine
    System.Collections.IEnumerator NextRoundRoutine()
    {
        // timer before next round
        yield return new WaitForSeconds(5f);

        if (p1Wins >= roundsToWin || p2Wins >= roundsToWin)
        {
            Debug.Log("GAME OVER! MATCH FINISHED");
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

        // reset timer
        currentTime = maxTime;
        isRunning = true;

        // back to positions
        player1Health.transform.position = p1StartPoint.position;
        player2Health.transform.position = p2StartPoint.position;

        // heal
        player1Health.CurrentHealth = player1Health.maxHealth;
        player2Health.CurrentHealth = player2Health.maxHealth;

        player1Health.enabled = true;
        player2Health.enabled = true;

        //reset anim
        player1Health.playerController.ResetState();
        player2Health.playerController.ResetState();

        if (fightIntro != null)
        {
            fightIntro.PlayIntroSequence(currentRound); 
        }
    }
}
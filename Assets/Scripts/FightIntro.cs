using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FightIntro : MonoBehaviour
{
    [Header("UI")]
    public GameObject roundLabel;
    public GameObject fightLabel;

    [Header("Time option")]
    public float timeBeforeFight = 1.5f; 
    public float fightDuration = 1.0f;  
    public float fadeSpeed = 0.2f;    

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip fullSound;

    private CanvasGroup roundCG;
    private CanvasGroup fightCG;
    
    // --- NEW: To store the players ---
    private PlayerController[] players; 

void Start()
    {
        roundCG = roundLabel.GetComponent<CanvasGroup>();
        fightCG = fightLabel.GetComponent<CanvasGroup>();

        if (roundCG == null || fightCG == null)
        {
            Debug.LogError("Error: Canvas Group missing!");
            return;
        }

        players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        
        Debug.Log("FightIntro found " + players.Length + " players.");

        if (players.Length == 0)
        {
             Debug.LogError("CRITICAL ERROR: FightIntro could not find any PlayerController scripts in the scene!");
        }

        SetPlayersLock(true); 

        StartCoroutine(StartFightSequence());
    }

    IEnumerator StartFightSequence()
    {
        roundCG.alpha = 0;
        fightCG.alpha = 0;
        roundLabel.SetActive(true);
        fightLabel.SetActive(true);

        if (audioSource != null && fullSound != null)
        {
            audioSource.PlayOneShot(fullSound);
        }

        // Show "Round 1"
        StartCoroutine(FadeCanvasGroup(roundCG, 0, 1, fadeSpeed));
        yield return new WaitForSeconds(timeBeforeFight);

        // Hide "Round 1", Show "Fight!"
        roundCG.alpha = 0;
        StartCoroutine(FadeCanvasGroup(fightCG, 0, 1, fadeSpeed));

        // --- NEW: UNLOCK PLAYERS NOW (When "Fight" appears) ---
        SetPlayersLock(false); 
        // ------------------------------------------------------

        yield return new WaitForSeconds(fightDuration);

        // Fade out "Fight!"
        StartCoroutine(FadeCanvasGroup(fightCG, 1, 0, fadeSpeed));
        yield return new WaitForSeconds(fadeSpeed);

        roundLabel.SetActive(false);
        fightLabel.SetActive(false);
    }

    // --- NEW: Helper function ---
    void SetPlayersLock(bool isLocked)
    {
        if (players != null)
        {
            foreach (PlayerController player in players)
            {
                if (player != null)
                {
                    player.SetControlLock(isLocked);
                }
            }
        }
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, timer / duration);
            yield return null;
        }
        cg.alpha = end;
    }
}
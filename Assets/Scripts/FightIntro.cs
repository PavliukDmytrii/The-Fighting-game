using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FightIntro : MonoBehaviour
{
    [Header("UI References")]
    public GameObject roundLabel;
    public Image roundNumberImage;
    public GameObject fightLabel;

    [Header("Assets")]
    public Sprite[] numberSprites; 

    [Header("Time Settings")]
    public float timeBeforeFight = 1.5f;
    public float fightDuration = 1.0f;
    public float fadeSpeed = 0.2f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip fullSound;

    private CanvasGroup roundCG;
    private CanvasGroup fightCG;
    private PlayerController[] players;

    void Awake()
    {
        roundCG = roundLabel.GetComponent<CanvasGroup>();
        fightCG = fightLabel.GetComponent<CanvasGroup>();

        players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

       
        if (roundLabel != null) roundLabel.SetActive(false);
        if (fightLabel != null) fightLabel.SetActive(false);
    }

   
    public void PlayIntroSequence(int roundNumber)
    {
        //(Round 1 -> Round 2)
        if (roundNumberImage != null && numberSprites != null && roundNumber < numberSprites.Length)
        {
            roundNumberImage.sprite = numberSprites[roundNumber];
            roundNumberImage.SetNativeSize(); 
        }

        // ----------------------------
        SetPlayersLock(true);
        // ----------------------------
        StopAllCoroutines();
        StartCoroutine(StartFightSequence());
    }

    IEnumerator StartFightSequence()
    {
        if (roundCG != null) roundCG.alpha = 0;
        if (fightCG != null) fightCG.alpha = 0;

        roundLabel.SetActive(true);
        fightLabel.SetActive(true);

        if (audioSource != null && fullSound != null)
        {
            audioSource.PlayOneShot(fullSound);
        }

 
        yield return StartCoroutine(FadeCanvasGroup(roundCG, 0, 1, fadeSpeed));
        yield return new WaitForSeconds(timeBeforeFight);

        
        roundCG.alpha = 0;
        yield return StartCoroutine(FadeCanvasGroup(fightCG, 0, 1, fadeSpeed));

        // ----------------------------
        SetPlayersLock(false);
        // ----------------------------

        yield return new WaitForSeconds(fightDuration);

       
        yield return StartCoroutine(FadeCanvasGroup(fightCG, 1, 0, fadeSpeed));

        roundLabel.SetActive(false);
        fightLabel.SetActive(false);
    }

    void SetPlayersLock(bool isLocked)
    {
        if (players != null)
        {
            foreach (PlayerController player in players)
            {
                if (player != null) player.SetControlLock(isLocked);
            }
        }
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        if (cg == null) yield break;

        float timer = 0f;
        float startAlpha = cg.alpha; 

        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, timer / duration);
            yield return null;
        }
        cg.alpha = end;
    }
}
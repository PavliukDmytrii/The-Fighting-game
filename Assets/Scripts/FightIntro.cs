using UnityEngine;
using UnityEngine.UI; 
using System.Collections;

public class FightIntro : MonoBehaviour
{
    [Header("UI")]
    public GameObject roundLabel;
    public GameObject fightLabel;

    [Header("Time options")]
    public float timeBeforeFight = 1.5f; 
    public float fightDuration = 1.0f;
    public float fadeSpeed = 0.2f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip fullSound;

  
    private CanvasGroup roundCG;
    private CanvasGroup fightCG;

    void Start()
    {
      
        roundCG = roundLabel.GetComponent<CanvasGroup>();
        fightCG = fightLabel.GetComponent<CanvasGroup>();

       
        if (roundCG == null || fightCG == null)
        {
            Debug.LogError("ОШИБКА: Добавь компонент Canvas Group на объекты roundLabel и fightLabel!");
            return;
        }

        StartCoroutine(StartFightSequence());
    }

    IEnumerator StartFightSequence()
    {
      
        roundCG.alpha = 0;
        fightCG.alpha = 0;
        roundLabel.SetActive(true);
        fightLabel.SetActive(true);

        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);


        foreach (PlayerController player in players)
        {
            player.enabled = false;
        }

 
        if (audioSource != null && fullSound != null)
        {
            audioSource.PlayOneShot(fullSound);
        }

 
        StartCoroutine(FadeCanvasGroup(roundCG, 0, 1, fadeSpeed));

        
        yield return new WaitForSeconds(timeBeforeFight);

  
        roundCG.alpha = 0;
        StartCoroutine(FadeCanvasGroup(fightCG, 0, 1, fadeSpeed)); 

     
        yield return new WaitForSeconds(fightDuration);

        
        StartCoroutine(FadeCanvasGroup(fightCG, 1, 0, fadeSpeed));


        yield return new WaitForSeconds(fadeSpeed);

    
        roundLabel.SetActive(false);
        fightLabel.SetActive(false);

        foreach (PlayerController player in players)
        {
            player.enabled = true;
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
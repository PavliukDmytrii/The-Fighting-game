using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro; 

public class VersusController : MonoBehaviour
{
    [Header("Options")]
    public TextMeshProUGUI pressEnterText; 
    public string sceneToLoad = "GameScene"; 
    public float blinkInterval = 0.5f; 

    private float timer;
    private float totalTimer;
    private float autoStartDelay = 24.0f;

    void Update()
    {
        
        timer += Time.deltaTime; 
        if (timer >= blinkInterval)
        {
            
            pressEnterText.enabled = !pressEnterText.enabled;
            timer = 0;
        }

        totalTimer += Time.deltaTime;

        if (totalTimer >= autoStartDelay)
        {
            SceneManager.LoadScene(sceneToLoad);
        }

            if (Input.GetKeyDown(KeyCode.Return))
            {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
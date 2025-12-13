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
        // We will add the winner logic here
    }
}
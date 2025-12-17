using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarUI : MonoBehaviour
{
    public Health targetHealthScript;
    [SerializeField] private float animationSpeed = 0.2f;

    private Slider healthSlider;
    private Image fillImage;
    private Coroutine animationCoroutine;

    void Start()
    {
        healthSlider = GetComponent<Slider>();

        Transform fillTransform = transform.Find("Fill Area/Fill");
        if (fillTransform != null)
        {
            fillImage = fillTransform.GetComponent<Image>();
        }

        if (targetHealthScript != null)
        {
            healthSlider.maxValue = targetHealthScript.maxHealth;
            healthSlider.value = targetHealthScript.CurrentHealth;
            UpdateColor(targetHealthScript.CurrentHealth);

            targetHealthScript.onHealthChanged.AddListener(UpdateHealthBar);
        }
    }

    public void UpdateHealthBar(int newHealth)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimateHealthBar(newHealth));
    }

    private IEnumerator AnimateHealthBar(int targetValue)
    {
        float startValue = healthSlider.value;
        float elapsed = 0f;

        while (elapsed < animationSpeed)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / animationSpeed;
            healthSlider.value = Mathf.Lerp(startValue, targetValue, percent);
            UpdateColor(healthSlider.value);

            yield return null;
        }

        healthSlider.value = targetValue;
        UpdateColor(targetValue);
    }

    private void UpdateColor(float currentValue)
    {
        if (fillImage != null)
        {
            float healthRatio = currentValue / healthSlider.maxValue;
            fillImage.color = Color.Lerp(Color.red, Color.green, healthRatio);
        }
    }

    private void OnDestroy()
    {
        if (targetHealthScript != null)
        {
            targetHealthScript.onHealthChanged.RemoveListener(UpdateHealthBar);
        }
    }
}
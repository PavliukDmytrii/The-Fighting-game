using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Health targetHealthScript;

    private Slider healthSlider;
    private Image fillImage;

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

            UpdateHealthBar(targetHealthScript.CurrentHealth);

            targetHealthScript.onHealthChanged.AddListener(UpdateHealthBar);
        }
        else
        {
            Debug.LogError("Target Health Script is not assigned to the HealthBarUI script on " + gameObject.name);
        }
    }

    public void UpdateHealthBar(int newHealth)
    {
        healthSlider.value = newHealth;

        if (fillImage != null)
        {
            float healthRatio = (float)newHealth / healthSlider.maxValue;

            fillImage.color = Color.Lerp(Color.yellow, Color.yellow, healthRatio);
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
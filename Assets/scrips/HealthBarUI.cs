using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Put this on the HealthBar (the Slider object).
/// PlayerHealth calls SetHealth(current, max) and this updates the bar.
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    public static HealthBarUI Instance { get; private set; }

    [SerializeField] private Slider slider;            // the bar itself (found automatically if left empty)
    [SerializeField] private Image fillImage;          // the "Fill" child, so the bar can change color
    [SerializeField] private TextMeshProUGUI valueText; // OPTIONAL: shows "55 / 100". Leave empty if you don't want it.

    [SerializeField] private Color fullColor = new Color(0.2f, 0.8f, 0.2f);   // green
    [SerializeField] private Color emptyColor = new Color(0.85f, 0.15f, 0.15f); // red

    private void Awake()
    {
        Instance = this;
        if (slider == null) slider = GetComponent<Slider>();
    }

    public void SetHealth(float current, float max)
    {
        if (max <= 0f) return;

        float ratio = Mathf.Clamp01(current / max);   // 1 = full health, 0 = dead
        slider.value = ratio;

        if (fillImage != null)
            fillImage.color = Color.Lerp(emptyColor, fullColor, ratio);

        if (valueText != null)
            valueText.text = Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(max);
    }
}
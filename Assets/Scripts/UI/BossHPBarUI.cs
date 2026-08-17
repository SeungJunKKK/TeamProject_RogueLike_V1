using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHPBarUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;

    private float currentMaxHp;

    public void InitHPBar(float maxHp)
    {
        currentMaxHp = maxHp;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = maxHp;
        }
        UpdateHPText(maxHp, maxHp);
    }

    public void UpdateHPBar(float currentHp)
    {
        if (hpSlider != null)
        {
            hpSlider.value = Mathf.Max(0f, currentHp);
        }
        UpdateHPText(currentHp, currentMaxHp);
    }

    private void UpdateHPText(float current, float max)
    {
        if (hpText != null)
        {
            hpText.text = $"{Mathf.Max(0f, current):F0} / {max:F0}";
        }
    }
}
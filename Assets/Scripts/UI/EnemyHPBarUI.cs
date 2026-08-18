using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHPBar : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Image fillImage;

    [Header("Color Thresholds")]
    [SerializeField] private Color greenColor = new Color(0.2f, 0.8f, 0.2f);  // 75% ~ 100%
    [SerializeField] private Color yellowColor = new Color(0.9f, 0.9f, 0.2f); // 55% ~ 75%
    [SerializeField] private Color orangeColor = new Color(1.0f, 0.5f, 0.0f); // 30% ~ 55%
    [SerializeField] private Color redColor = new Color(0.9f, 0.2f, 0.2f);    // 0% ~ 30%
    [SerializeField] private Vector3 m_Offset = new Vector3(0f, 0.5f, 0f); // FlyingEnemy 오프셋 고정용

    // FlyingEnemy가 회전하더라도 Canvas 회전을 정방향(0도)으로 강제 고정
    private void LateUpdate()
    {
        transform.rotation = Quaternion.identity;

        if (transform.parent != null)
        {
            transform.position = transform.parent.position + m_Offset;
        }
    }

    public void UpdateHPBar(float currentHp, float maxHp)
    {
        if (maxHp <= 0f) return;

        float ratio = Mathf.Clamp01(currentHp / maxHp);

        if (hpSlider != null)
        {
            hpSlider.minValue = 0f;
            hpSlider.maxValue = 1f;
            hpSlider.value = ratio;
        }

        if (fillImage != null)
        {
            fillImage.color = GetColorByRatio(ratio);
        }
    }

    private Color GetColorByRatio(float ratio)
    {
        if (ratio >= 0.75f)
        {
            return greenColor;
        }
        else if (ratio >= 0.55f)
        {
            return yellowColor;
        }
        else if (ratio >= 0.30f)
        {
            return orangeColor;
        }
        else
        {
            return redColor;
        }
    }
}
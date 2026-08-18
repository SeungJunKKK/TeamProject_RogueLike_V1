using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSlotUI : MonoBehaviour
{
    [Header("Skill Settings")]
    [SerializeField] private SkillType skillType;

    [Header("UI Reference")]
    [SerializeField] private Image skillIcon;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TMP_Text cooldownText;

    public SkillType SkillType => skillType;

    private void Awake()
    {
        ResetCooldownUI();
    }

    public void SetSkill(SkillInfo info)
    {
        if (skillIcon != null)
        {
            skillIcon.sprite = info.SkillIcon;
            skillIcon.enabled = (info.SkillIcon != null);
        }
    }

    public void UpdateCooldown(float remainingTime, float maxCooldown)
    {
        // 남은 쿨타임이 0초보다 크면 UI 표시
        if (remainingTime > 0f && maxCooldown > 0f)
        {
            if (cooldownOverlay != null && !cooldownOverlay.gameObject.activeSelf)
                cooldownOverlay.gameObject.SetActive(true);

            if (cooldownText != null && !cooldownText.gameObject.activeSelf)
                cooldownText.gameObject.SetActive(true);

            // 오버레이 게이지 비율 계산
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = Mathf.Clamp01(remainingTime / maxCooldown);
            }

            // 남은 초 정수 출력
            if (cooldownText != null)
            {
                cooldownText.text = Mathf.CeilToInt(remainingTime).ToString();
            }
        }
        else
        {
            // 쿨타임 종료 시 UI 숨김
            ResetCooldownUI();
        }
    }

    private void ResetCooldownUI()
    {
        if (cooldownOverlay != null) cooldownOverlay.gameObject.SetActive(false);
        if (cooldownText != null) cooldownText.gameObject.SetActive(false);
    }
}
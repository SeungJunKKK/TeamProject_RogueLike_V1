using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UseItemSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage; // 아이템 아이콘 표시 이미지
    [SerializeField] private Image cooldownOverlay; // 쿨타임 Fill Image
    [SerializeField] private TMP_Text cooldownText; // 남은 쿨타임 숫자 텍스트

    [Header("Buff UI Reference")]
    [SerializeField] private GameObject healBuffUI; // Buff_Heal 오브젝트 할당

    private float buffDurationTimer = 0f;

    private void Update()
    {
        // 버프 UI 지속 시간 타이머 처리
        if (buffDurationTimer > 0f)
        {
            buffDurationTimer -= Time.deltaTime;
            if (buffDurationTimer <= 0f)
            {
                if (healBuffUI != null) healBuffUI.SetActive(false); // 버프 종료 시 UI 비활성화
            }
        }
    }

    public void ShowHealBuff(float duration) // 지속 시간 지정
    {
        if (healBuffUI != null)
        {
            healBuffUI.SetActive(true);
            buffDurationTimer = duration; // 지속 시간 세팅
        }
    }

    private void Awake()
    {
        ResetSlotUI();
    }

    public void SetItem(Sprite icon) // 아이템을 획득했을 때 UI 등록
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = true; // 아이콘 이미지 활성화
            Color color = iconImage.color;
            color.a = 1f;
            iconImage.color = color;
        }

        ResetCooldownUI();
    }

    // 아이템 사용 후 / 빈 슬롯 초기화
    public void ClearItem()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        ResetCooldownUI();
    }

    // 쿨타임 매 프레임 업데이트
    public void UpdateCooldown(float remainingTime, float maxCooldown)
    {
        if (remainingTime > 0f && maxCooldown > 0f)
        {
            if (cooldownOverlay != null && !cooldownOverlay.gameObject.activeSelf)
                cooldownOverlay.gameObject.SetActive(true);

            if (cooldownText != null && !cooldownText.gameObject.activeSelf)
                cooldownText.gameObject.SetActive(true);

            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = Mathf.Clamp01(remainingTime / maxCooldown);

            if (cooldownText != null)
                cooldownText.text = Mathf.CeilToInt(remainingTime).ToString();
        }
        else
        {
            ResetCooldownUI();
        }
    }

    private void ResetCooldownUI()
    {
        if (cooldownOverlay != null) cooldownOverlay.gameObject.SetActive(false);
        if (cooldownText != null) cooldownText.gameObject.SetActive(false);
    }

    private void ResetSlotUI()
    {
        ResetCooldownUI();
        if (iconImage != null && iconImage.sprite == null)
        {
            iconImage.enabled = false;
        }
    }
}
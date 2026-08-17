using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private Image hpBarFill; // 초록색 체력바
    [SerializeField] private TMP_Text hpText; // 체력 텍스트

    [Header("EXP & Level UI Reference")]
    [SerializeField] private Image expBarFill; // 경험치바
    [SerializeField] private TMP_Text levelText; // 레벨 텍스트

    private PlayerStats playerStats;

    private void OnEnable()
    {
        EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerHpChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerHpChanged);
    }

    private void Start()
    {
        // 게임 시작 시 초기 체력 UI
        InitPlayerUI();
    }

    private void Update()
    {
        // 경험치 및 레벨 UI 실시간 반영
        if (playerStats != null)
        {
            UpdateHpUI(playerStats.CurrentHealth, playerStats.MaxHealth.Value);
            UpdateExpAndLevelUI();
        }
    }

    private void InitPlayerUI()
    {
        // PlayerStats 컴포넌트 찾기
        playerStats = FindAnyObjectByType<PlayerStats>();

        if (playerStats != null)
        {
            // 체력 UI 초기화
            UpdateHpUI(playerStats.CurrentHealth, playerStats.MaxHealth.Value);
            // 경험치 및 레벨 UI 초기화
            UpdateExpAndLevelUI();
        }
        else
        {
            Debug.LogWarning("PlayerStats 찾을 수 없음");
        }
    }

    private void OnPlayerHpChanged(PlayerDamagedEvent e)
    {
        UpdateHpUI(e.CurrentHp, e.MaxHp);
    }

    public void UpdateHpUI(float currentHp, float maxHp)
    {
        if (maxHp <= 0) return;

        // HP Fill 계산
        float fillRatio = Mathf.Clamp01(currentHp / maxHp);
        if (hpBarFill != null)
        {
            hpBarFill.fillAmount = fillRatio;
        }

        // 체력 텍스트 출력
        if (hpText != null)
        {
            hpText.text = $"{Mathf.CeilToInt(currentHp)}/{Mathf.CeilToInt(maxHp)}";
        }
    }

    private void UpdateExpAndLevelUI()
    {
        // playerStats가 null이면 중단
        if (playerStats == null) return;

        // 경험치 게이지 계산
        float reqExp = playerStats.GetRequiredExp(playerStats.CurrentLevel);
        if (reqExp > 0 && expBarFill != null)
        {
            float expRatio = Mathf.Clamp01(playerStats.CurrentExp / reqExp);
            expBarFill.fillAmount = expRatio;
        }

        // 레벨 텍스트 갱신
        if (levelText != null)
        {
            levelText.text = $"{playerStats.CurrentLevel}";
        }
    }
}
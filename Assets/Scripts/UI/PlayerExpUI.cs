using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUDUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text lvText;
    [SerializeField] private Image exFill;

    [Header("Player Reference")]
    [SerializeField] private PlayerController player;

    private void Start()
    {
        // 인스펙터에 플레이어가 연결되어 있지 않다면 자동으로 찾음
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>();
        }
    }

    private void Update()
    {
        UpdateExpUI();
    }

    private void UpdateExpUI()
    {
        // 플레이어 또는 PlayerStats 정보가 없으면 리턴
        if (player == null || player.Stats == null)
        {
            // 플레이어를 재탐색
            player = FindAnyObjectByType<PlayerController>();
            return;
        }

        PlayerStats stats = player.Stats;

        // 레벨 텍스트 반영
        if (lvText != null)
        {
            lvText.text = stats.CurrentLevel.ToString();
        }

        // 경험치 게이지 바 반영
        if (exFill != null)
        {
            float currentExp = stats.CurrentExp;
            float requiredExp = stats.GetRequiredExp(stats.CurrentLevel);

            if (requiredExp > 0f)
            {
                exFill.fillAmount = Mathf.Clamp01(currentExp / requiredExp);
            }
            else
            {
                exFill.fillAmount = 1f;
            }
        }
    }
}
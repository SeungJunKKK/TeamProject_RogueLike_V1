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

    [Header("Level Up Effect Settings")]
    [SerializeField] private LevelUpEffect levelUpEffectPrefab; // 레벨업 이펙트 프리팹
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 1.5f, 0f); // 플레이어 머리 위 오프셋
    private int lastLevel = -1; // 이전 프레임의 레벨 저장용
    private void Start()
    {
        // 인스펙터에 플레이어가 연결되어 있지 않다면 자동으로 찾음
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>();
        }

        // 초기 레벨 설정
        if (player != null && player.Stats != null)
        {
            lastLevel = player.Stats.CurrentLevel;
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
            if (player != null && player.Stats != null && lastLevel == -1)
            {
                lastLevel = player.Stats.CurrentLevel;
            }
            return;
        }

        PlayerStats stats = player.Stats;

        // 초기화가 안 되어 있다면 레벨 저장
        if (lastLevel == -1)
        {
            lastLevel = stats.CurrentLevel;
        }

        if (stats.CurrentLevel > lastLevel)
        {
            SpawnLevelUpEffect();
            lastLevel = stats.CurrentLevel; // 최신 레벨로 갱신
        }
        // 만약 레벨이 감소하는 특수 케이스가 있다면 수치만 동기화
        else if (stats.CurrentLevel < lastLevel)
        {
            lastLevel = stats.CurrentLevel;
        }

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

    private void SpawnLevelUpEffect()
    {
        if (levelUpEffectPrefab == null || player == null) return;

        Vector3 spawnPos = player.transform.position + spawnOffset;
        LevelUpEffect effectInstance = Instantiate(levelUpEffectPrefab, spawnPos, Quaternion.identity);
        effectInstance.PlayEffect(spawnPos);
    }
}
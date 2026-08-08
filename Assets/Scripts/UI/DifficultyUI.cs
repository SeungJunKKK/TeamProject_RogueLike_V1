using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DifficultyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private Image[] levelSlots;

    [Header("Level Up Effect Settings")]
    [SerializeField] private GameObject levelUpEffectPrefab; // 2단계에서 만든 이펙트 프리팹
    [SerializeField] private Transform playerTransform; // 플레이어 Transform
    [SerializeField] private Vector3 headOffset = new Vector3(0, 1.5f, 0); // 머리 위 오프셋 위치

    private EDifficultyLevel lastLevel;

    private void Start()
    {
        if (DifficultyManager.Instance != null)
        {
            lastLevel = DifficultyManager.Instance.Level;
        }

        // 만약 Inspector에서 Player를 안 넣어두었다면 태그로 찾아 연결
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (DifficultyManager.Instance == null) return;

        UpdateTimerUI();
        UpdateDifficultyBarUI();

        // 레벨이 상승했는지 체크
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        EDifficultyLevel currentLevel = DifficultyManager.Instance.Level;

        if (currentLevel > lastLevel)
        {
            // 레벨업 발생!
            SpawnLevelUpEffect();
            lastLevel = currentLevel;
        }
        else if (currentLevel < lastLevel)
        {
            lastLevel = currentLevel;
        }
    }

    private void SpawnLevelUpEffect()
    {
        if (levelUpEffectPrefab == null) return;

        Vector3 spawnPos = Vector3.zero;
        if (playerTransform != null)
        {
            spawnPos = playerTransform.position + headOffset;
        }

        GameObject effectObj = Instantiate(levelUpEffectPrefab, spawnPos, Quaternion.identity);
        LevelUpEffect effectScript = effectObj.GetComponent<LevelUpEffect>();

        if (effectScript != null)
        {
            effectScript.PlayEffect(spawnPos);
        }
    }

    private void UpdateTimerUI()
    {
        if (timeText == null) return;
        float totalSeconds = DifficultyManager.Instance.ElapsedSeconds;
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void UpdateDifficultyBarUI()
    {
        if (difficultyText != null)
        {
            difficultyText.text = DifficultyManager.Instance.Level.ToString();
        }

        if (levelSlots == null || levelSlots.Length == 0) return;

        int currentLevelIndex = (int)DifficultyManager.Instance.Level;
        float currentProgress = DifficultyManager.Instance.GetLevelProgress();

        for (int i = 0; i < levelSlots.Length; i++)
        {
            if (levelSlots[i] == null) continue;

            if (i < currentLevelIndex)
            {
                levelSlots[i].fillAmount = 1f;
            }
            else if (i == currentLevelIndex)
            {
                levelSlots[i].fillAmount = currentProgress;
            }
            else
            {
                levelSlots[i].fillAmount = 0f;
            }
        }
    }
}
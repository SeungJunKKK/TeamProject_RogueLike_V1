using UnityEngine;
using TMPro;

public class GameResultUI : MonoBehaviour
{
    [Header("Title & Class")]
    [SerializeField] private TextMeshProUGUI m_TitleText; // "WON!" 또는 "DIED"
    [SerializeField] private TextMeshProUGUI m_ClassText; 

    [Header("Stats & Points Text (Value / Points)")]
    [SerializeField] private TextMeshProUGUI m_LevelValueText;
    [SerializeField] private TextMeshProUGUI m_LevelPointsText;

    [SerializeField] private TextMeshProUGUI m_TimeValueText;
    [SerializeField] private TextMeshProUGUI m_TimePointsText;

    [SerializeField] private TextMeshProUGUI m_KillsValueText;
    [SerializeField] private TextMeshProUGUI m_KillsPointsText;

    [SerializeField] private TextMeshProUGUI m_BossValueText;
    [SerializeField] private TextMeshProUGUI m_BossPointsText;

    [SerializeField] private TextMeshProUGUI m_ItemValueText;
    [SerializeField] private TextMeshProUGUI m_ItemPointsText;

    [SerializeField] private TextMeshProUGUI m_GoldValueText;
    [SerializeField] private TextMeshProUGUI m_GoldPointsText;

    [SerializeField] private TextMeshProUGUI m_PurchaseValueText;
    [SerializeField] private TextMeshProUGUI m_PurchasePointsText;

    [SerializeField] private TextMeshProUGUI m_TotalScoreText;

    [Header("New Record Tags")]
    [SerializeField] private GameObject m_NewRecordTag_Level;
    [SerializeField] private GameObject m_NewRecordTag_Time;
    [SerializeField] private GameObject m_NewRecordTag_Kills;
    [SerializeField] private GameObject m_NewRecordTag_Boss;
    [SerializeField] private GameObject m_NewRecordTag_Item;
    [SerializeField] private GameObject m_NewRecordTag_Gold;
    [SerializeField] private GameObject m_NewRecordTag_Total;

    [Header("Unlock Panel")]
    [SerializeField] private Transform m_UnlockGridTransform;
    [SerializeField] private GameObject m_UnlockIconPrefab;
    private void Awake()
    {
        EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDiedEventReceived);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDiedEventReceived);
    }

    private void OnPlayerDiedEventReceived(PlayerDiedEvent e)
    {
        DisplayResult(false);
    }

    public void DisplayResult(bool isWin)
    {
        gameObject.SetActive(true);
        m_TitleText.text = isWin ? "WON!" : "DIED";

        CurrentRunData run = SaveLoadManager.Instance.CurrentRun;
        PlayerSaveData save = SaveLoadManager.Instance.CurrentSave;

        // 1. 직업 이름 출력
        m_ClassText.text = $"Class: {run.ClassName}";

        // 2. 각 항목별 수치 및 포인트 계산 대입
        // Level
        int levelPoints = run.CurrentLevel * 500;
        m_LevelValueText.text = run.CurrentLevel.ToString();
        m_LevelPointsText.text = $"{levelPoints} POINTS";

        // Time (분:초 변환 형식)
        int minutes = Mathf.FloorToInt(run.PlayTime / 60f);
        int seconds = Mathf.FloorToInt(run.PlayTime % 60f);
        m_TimeValueText.text = $"{minutes}:{seconds:D2}";
        int timePoints = Mathf.FloorToInt(run.PlayTime) * 3;
        m_TimePointsText.text = $"{timePoints} POINTS";

        // Kills
        int killPoints = run.Kills * 100;
        m_KillsValueText.text = run.Kills.ToString();
        m_KillsPointsText.text = $"{killPoints} POINTS";

        // Bosses
        int bossPoints = run.BossesKilled * 1000;
        m_BossValueText.text = run.BossesKilled.ToString();
        m_BossPointsText.text = $"{bossPoints} POINTS";

        // Items
        int itemPoints = run.ItemsCollected * 110;
        m_ItemValueText.text = run.ItemsCollected.ToString();
        m_ItemPointsText.text = $"{itemPoints} POINTS";

        // Gold
        int goldPoints = run.GoldCollected * 2;
        m_GoldValueText.text = run.GoldCollected.ToString();
        m_GoldPointsText.text = $"{goldPoints} POINTS";

        // Purchases
        int purchasePoints = run.Purchases * 35;
        m_PurchaseValueText.text = run.Purchases.ToString();
        m_PurchasePointsText.text = $"{purchasePoints} POINTS";

        // 3. 총 점수 계산
        int currentTotalScore = run.CalculateTotalScore();
        m_TotalScoreText.text = currentTotalScore.ToString();

        // ==========================================
        //  4. NEW RECORD 비교 
        // ==========================================
        CheckAndUpdateRecord(run.Kills, ref save.MaxKillsInOneRun, m_NewRecordTag_Kills);
        CheckAndUpdateRecord(run.GoldCollected, ref save.MaxGoldCollected, m_NewRecordTag_Gold);
        CheckAndUpdateRecord(currentTotalScore, ref save.HighestTotalScore, m_NewRecordTag_Total);

        // 5. 이번 판 신규 해금 아이템 우측 그리드에 생성
        foreach (string itemID in run.NewlyUnlockedItems)
        {
            ItemData itemData = Resources.Load<ItemData>($"Items/{itemID}");

            if (itemData != null)
            {
                if (m_UnlockIconPrefab != null && m_UnlockGridTransform != null)
                {
                    GameObject iconObj = Instantiate(m_UnlockIconPrefab, m_UnlockGridTransform);

                    if (iconObj.TryGetComponent(out ResultItemSlot slot))
                    {
                        slot.Setup(itemData); 
                    }
                }
            }
            else
            {
                Debug.LogWarning($"<color=orange>[GameResultUI] 아이템 데이터 아티팩트를 찾지 못했습니다: Items/{itemID}</color>");
            }
        }

        // 6. 최종 세이브 저장
        SaveLoadManager.Instance.SaveGame();
    }

    // 중복되는 NEW RECORD 판별 코드를 줄여주는  헬퍼 메서드
    private void CheckAndUpdateRecord(int currentValue, ref int savedMaxValue, GameObject recordTagObject)
    {
        if (currentValue > savedMaxValue)
        {
            if (recordTagObject != null) recordTagObject.SetActive(true);
            savedMaxValue = currentValue;
        }
        else
        {
            if (recordTagObject != null) recordTagObject.SetActive(false);
        }
    }
}
using UnityEngine;
using TMPro;

public class GoldUI : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;

    private void Awake()
    {
        // 텍스트 컴포넌트 자동 할당
        if (moneyText == null)
        {
            moneyText = GetComponentInChildren<TMP_Text>();
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);

        // UI 활성화 시 현재 골드 값으로 즉시 초기화
        if (GameManager.Instance != null)
        {
            UpdateGoldUI(GameManager.Instance.Gold);
        }
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
    }

    private void OnGoldChanged(GoldChangedEvent e)
    {
        UpdateGoldUI(e.Current);
    }

    private void UpdateGoldUI(int currentGold)
    {
        if (moneyText != null)
        {
            moneyText.text = currentGold.ToString();
        }
    }
}
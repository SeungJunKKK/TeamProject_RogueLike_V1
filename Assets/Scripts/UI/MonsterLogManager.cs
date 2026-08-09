using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterLogManager : MonoBehaviour
{
    [Header("Addressables Asset Key")]
    [SerializeField] private string monsterSpriteSheetKey = "MonsterLog";

    [Header("UI Component References")]
    [SerializeField] private Transform monsterGridContainer;
    [SerializeField] private GameObject monsterSlotPrefab;

    [Header("Right Page Details")]
    [SerializeField] private TextMeshProUGUI monsterNameText;
    [SerializeField] private Image monsterDetailImage;
    [SerializeField] private TextMeshProUGUI monsterDescriptionText;

    // Description
    private readonly string defaultGuideText =
        "Monster Logs contain information, lore, and custom art of enemies found on the planet.\n\n" +
        "Monster Log entries have a small chance of dropping whenever you kill an enemy.\n\nGood hunting!";

    private void OnEnable()
    {
        ResetRightPage();
        LoadMonsterLogs();
    }

    private void LoadMonsterLogs()
    {
        // 기존 Grid 아이템 초기화
        foreach (Transform child in monsterGridContainer)
        {
            Destroy(child.gameObject);
        }

        // AddressableManager를 통해 몬스터 스프라이트 시트 전체 로드
        AddressableManager.Instance.LoadSpriteSheetAsync(monsterSpriteSheetKey, (sprites) =>
        {
            if (sprites == null || sprites.Count == 0)
            {
                Debug.LogWarning($"[MonsterLogManager] 몬스터 스프라이트를 불러오지 못했습니다: {monsterSpriteSheetKey}");
                return;
            }

            foreach (Sprite sprite in sprites)
            {
                CreateMonsterSlot(sprite);
            }
        });
    }

    private void CreateMonsterSlot(Sprite sprite)
    {
        GameObject slotObj = Instantiate(monsterSlotPrefab, monsterGridContainer);

        Image slotImage = slotObj.GetComponent<Image>();
        if (slotImage == null) slotImage = slotObj.GetComponentInChildren<Image>();

        if (slotImage != null)
        {
            slotImage.sprite = sprite;
            slotImage.color = Color.white;
        }

        // 슬롯 클릭 이벤트 바인딩
        Button slotBtn = slotObj.GetComponent<Button>();
        if (slotBtn != null)
        {
            slotBtn.onClick.RemoveAllListeners();
            slotBtn.onClick.AddListener(() => OnSelectMonster(sprite));
        }
    }

    private void OnSelectMonster(Sprite selectedSprite)
    {
        // 클릭한 몬스터 정보를 우측 페이지에 표시
        if (monsterDetailImage != null)
        {
            monsterDetailImage.gameObject.SetActive(true);
            monsterDetailImage.sprite = selectedSprite;
        }

        if (monsterNameText != null)
        {
            // 스프라이트 에셋 이름을 몬스터 이름으로 활용 (필요시 데이터 테이블과 연동)
            monsterNameText.text = selectedSprite.name;
        }

        if (monsterDescriptionText != null)
        {
            // 몬스터 도감 설명
            monsterDescriptionText.text = $"Detailed lore and stats for {selectedSprite.name}.";
        }
    }

    private void ResetRightPage()
    {
        if (monsterNameText != null) monsterNameText.text = "Monster Log";
        if (monsterDetailImage != null) monsterDetailImage.gameObject.SetActive(false);
        if (monsterDescriptionText != null) monsterDescriptionText.text = defaultGuideText;
    }
}
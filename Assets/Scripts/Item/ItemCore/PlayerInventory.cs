using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // 아이템 데이터를 키(Key)로, 획득한 개수를 값(Value)으로 저장 딕셔너리
    public Dictionary<ItemData, int> passiveItems = new Dictionary<ItemData, int>(); //[cite: 7]

    [Header("Active Item Slot")]
    public ItemData currentActiveItem;
    private float m_ActiveCooldownTimer = 0f;

    private PlayerController player;
    private PlayerStats stats;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (m_ActiveCooldownTimer > 0f)
        {
            m_ActiveCooldownTimer -= Time.deltaTime;
        }

        // 키보드 'E'를 누르면 액티브 아이템을 사용하도록 설정 (추후 원하는 키로 변경 가능 => 매핑 시스템 )
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseActiveItem();
        }
    }

    // 아이템 획득 시 호출될 함수 (상자에서 아이템을 먹었을 때 실행됨)
    public void AddItem(ItemData newItem)
    {
        if (newItem.tier == ItemTier.Use)
        {
            currentActiveItem = newItem;
            m_ActiveCooldownTimer = 0f; 
            Debug.Log($"[Inventory] 액티브 아이템 장착: {newItem.itemName}");
            return;
        }

        if (passiveItems.ContainsKey(newItem))
        {
            if (newItem.maxStack == 0 || passiveItems[newItem] < newItem.maxStack)
            {
                passiveItems[newItem]++;
                //Debug.Log($"[Inventory] {newItem.itemName} 중첩  현재 개수: {passiveItems[newItem]}");
            }
        }
        else
        {
            passiveItems.Add(newItem, 1);
            Debug.Log($"[Inventory] {newItem.itemName} 획득!");
        }

        UpdatePassiveStats();
    }

    /// <summary>
    ///액티브 아이템 사용 실행 함수 
    /// </summary>
    public void UseActiveItem()
    {
        if (currentActiveItem == null)
        {
            return;
        }

        // 쿨타임 체크
        if (m_ActiveCooldownTimer > 0f)
        {
            Debug.Log($"<color=orange>[시스템] 아직 {currentActiveItem.itemName} 쿨타임입니다! (남은 시간: {m_ActiveCooldownTimer:F1}초)</color>");
            return;
        }

        // 아이템 사용
        if (currentActiveItem.OnUse(player))
        {
            m_ActiveCooldownTimer = currentActiveItem.cooldown;
        }
    }

    // =======================================================
    //  1. 패시브 스탯 일괄 재계산 함수
    // =======================================================
    private void UpdatePassiveStats()
    {
        // 인벤토리에 있는 모든 아이템을 순회하며 ApplyPassiveStat 실행
        foreach (var pair in passiveItems)
        {
            ItemData item = pair.Key;
            int count = pair.Value;
            item.ApplyPassiveStat(stats, count);
        }
    }

    // =======================================================
    // 2. 플레이어의 행동을 아이템들에게 전달 Trigger
    // =======================================================

    // 플레이어가 적을 때렸을 때
    public void OnHitEnemyTrigger(GameObject target, float damage)
    {
        foreach (var pair in passiveItems)
        {
            pair.Key.OnHitEnemy(player, target, damage, pair.Value);
        }
    }

    // 플레이어가 적을 죽였을 때
    public void OnKillEnemyTrigger(GameObject target)
    {
        foreach (var pair in passiveItems)
        {
            pair.Key.OnKillEnemy(player, target, pair.Value);
        }
    }

    // 플레이어가 피해를 입었을 때
    public void OnTakeDamageTrigger(float damageTaken)
    {
        foreach (var pair in passiveItems)
        {
            pair.Key.OnTakeDamage(player, damageTaken, pair.Value);
        }
    }

    // 플레이어가 기본 공격을 했을 때 호출할 트리거
    public void OnBasicAttackTrigger()
    {
        //Debug.Log("<color=cyan>[디버그] 1. PlayerInventory의 OnBasicAttackTrigger가 정상 호출</color>");
        foreach (var pair in passiveItems)
        {
            pair.Key.OnBasicAttack(player, pair.Value);
        }
    }


    // ==========================================
    // 치트/시스템용: 액티브 쿨타임 초기화 함수
    // ==========================================
    public void ResetActiveCooldown()
    {
        m_ActiveCooldownTimer = 0f;
        Debug.Log("<color=cyan>[시스템] 액티브 아이템 쿨타임이 초기화되었습니다!</color>");
    }

}
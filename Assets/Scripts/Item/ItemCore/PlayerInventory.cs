using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // 아이템 데이터를 키(Key)로, 획득한 개수를 값(Value)으로 저장 딕셔너리
    public Dictionary<ItemData, int> passiveItems = new Dictionary<ItemData, int>(); //[cite: 7]

    private PlayerController player;
    private PlayerStats stats;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        stats = GetComponent<PlayerStats>();
    }

    // 아이템 획득 시 호출될 함수 (상자에서 아이템을 먹었을 때 실행됨)
    public void AddItem(ItemData newItem)
    {
        // 1. 이미 가지고 있는 아이템이면 중첩 수 +1
        if (passiveItems.ContainsKey(newItem)) 
        {
            if (newItem.maxStack == 0 || passiveItems[newItem] < newItem.maxStack) 
            {
                passiveItems[newItem]++; 
                Debug.Log($"[Inventory] {newItem.itemName} 중첩  현재 개수: {passiveItems[newItem]}"); 
            }
        }
        // 2 처음 먹는 아이템이면 딕셔너리에 새로 등록 
        else
        {
            passiveItems.Add(newItem, 1);
            Debug.Log($"[Inventory] {newItem.itemName} 획득!"); 
        }

        //아이템을 획득 ->  패시브 스탯 갱신
        UpdatePassiveStats();
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
        Debug.Log("<color=cyan>[디버그] 1. PlayerInventory의 OnBasicAttackTrigger가 정상 호출</color>");
        foreach (var pair in passiveItems)
        {
            pair.Key.OnBasicAttack(player, pair.Value);
        }
    }
}
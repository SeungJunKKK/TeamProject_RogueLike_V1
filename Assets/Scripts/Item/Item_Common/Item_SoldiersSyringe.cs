using UnityEngine;

[CreateAssetMenu(fileName = "Item_SoldiersSyringe", menuName = "Items/Common/Soldiers Syringe")]
public class Item_SoldiersSyringe : ItemData
{
    [Header("Syringe Settings")]
    public float attackSpeedBoostPerStack = 0.15f; // 15%

    private void Awake()
    {
        tier = ItemTier.Common;
        maxStack = 13;
    }

    public override void ApplyPassiveStat(PlayerStats stats, int stackCount)
    {
        stats.AttackSpeed.RemoveAllModifiersFromSource(this);
        int actualStack = Mathf.Min(stackCount, maxStack);
        float totalBoost = attackSpeedBoostPerStack * actualStack;
        stats.AttackSpeed.AddModifier(new StatModifier(totalBoost, StatModType.PercentAdd, this));
        Debug.Log($"[Item] {itemName} 적용됨: 공격 속도 {totalBoost * 100}% 증가 (현재 중첩: {actualStack}/{maxStack})");
    }
}
using UnityEngine;

[CreateAssetMenu(fileName = "Item_MysteriousVial", menuName = "Items/Common/Mysterious Vial")]
public class Item_MysteriousVial : ItemData
{
    [Header("Vial Settings")]
    public float regenBoostPerStack = 1.2f; // 중첩당 초당 1.2 회복

    private void Awake()
    {
        tier = ItemTier.Common;
        maxStack = 0; // 무제한
    }

    public override void ApplyPassiveStat(PlayerStats stats, int stackCount)
    {
        stats.HealthRegen.RemoveAllModifiersFromSource(this);
        float totalBoost = regenBoostPerStack * stackCount;
        stats.HealthRegen.AddModifier(new StatModifier(totalBoost, StatModType.Flat, this));
        Debug.Log($"[Item] {itemName} 적용됨: 체력 재생력 {totalBoost} 증가 (현재 중첩: {stackCount})");
    }
}
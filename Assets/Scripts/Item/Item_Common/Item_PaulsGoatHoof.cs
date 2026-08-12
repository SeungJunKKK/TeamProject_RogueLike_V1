using UnityEngine;

[CreateAssetMenu(fileName = "Item_PaulsGoatHoof", menuName = "Items/Common/Pauls Goat Hoof")]
public class Item_PaulsGoatHoof : ItemData
{
    [Header("Goat Hoof Settings")]
    public float moveSpeedBoostPerStack = 0.20f; // 20% 증가

    private void Awake()
    {
        tier = ItemTier.Common;
        maxStack = 24; // 최대 24중첩
    }

    public override void ApplyPassiveStat(PlayerStats stats, int stackCount)
    {
        stats.MoveSpeed.RemoveAllModifiersFromSource(this);
        int actualStack = Mathf.Min(stackCount, maxStack);
        float totalBoost = moveSpeedBoostPerStack * actualStack;
        stats.MoveSpeed.AddModifier(new StatModifier(totalBoost, StatModType.PercentAdd, this));
        Debug.Log($"[Item] {itemName} 적용됨: 이동 속도 {totalBoost * 100}% 증가 (현재 중첩: {actualStack}/{maxStack})");
    }
}
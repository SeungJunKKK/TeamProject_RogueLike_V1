using UnityEngine;

[CreateAssetMenu(fileName = "Item_AlienHead", menuName = "Items/Rare/Alien Head")]
public class Item_AlienHead : ItemData
{
    [Header("Alien Head Settings")]
    public float cdrPerStack = 0.30f; 

    private void Awake()
    {
        tier = ItemTier.Rare;
        maxStack = 2; // 2중첩 시 60% 감소
    }

    public override void ApplyPassiveStat(PlayerStats stats, int stackCount)
    {
        stats.CooldownReduction.RemoveAllModifiersFromSource(this);
        int actualStack = Mathf.Min(stackCount, maxStack);
        float totalBoost = cdrPerStack * actualStack;
        stats.CooldownReduction.AddModifier(new StatModifier(totalBoost, StatModType.PercentAdd, this));
        Debug.Log($"[Item] {itemName} 적용됨: 스킬 쿨타임 {totalBoost * 100}% 감소 (현재 중첩: {actualStack}/{maxStack})");
    }
}
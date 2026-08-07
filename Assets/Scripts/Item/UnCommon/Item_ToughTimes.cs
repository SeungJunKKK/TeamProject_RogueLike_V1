using UnityEngine;

[CreateAssetMenu(fileName = "ToughTimes", menuName = "RiskOfRain/Items/Tough Times")]
public class Item_ToughTimes : ItemData
{
    public override void ApplyPassiveStat(PlayerStats stats, int stackCount)
    {
        // 혹시 이미 적용된 'Tough Times' 효과가 있다면 지움 (중복 적용 방지)
        stats.Armor.RemoveAllModifiersFromSource(this);

        if (stackCount > 0)
        {
            float bonusArmor = 14f * stackCount;

            // 새로운 모디파이어를 생성합니다. (수치, 연산 타입, 출처)
            StatModifier modifier = new StatModifier(bonusArmor, StatModType.Flat, this);

            stats.Armor.AddModifier(modifier);

            Debug.Log($"[Item] Tough Times 재적용: 방어력 {bonusArmor} 증가");
        }
    }
}
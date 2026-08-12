using UnityEngine;

[CreateAssetMenu(fileName = "Ukulele", menuName = "Items/UnCommon/Ukulele")]
public class Item_Ukulele : ItemData
{
    public override void OnHitEnemy(PlayerController player, GameObject target, float damage, int stackCount)
    {
        if (Random.value <= 0.2f)
        {
            float lightningDamage = damage * (0.33f * stackCount);
            Debug.Log($"[Item] 우쿨렐레 대상: {target.name}에게 {lightningDamage}의 연쇄 번개 피해");

            //Todo : 번개 이펙트 및 주변 적 타격 로직 
        }
    }
}
using UnityEngine;

[CreateAssetMenu(fileName = "Item_Crowbar", menuName = "Items/Common/Crowbar")]
public class Item_Crowbar : ItemData
{
    [Header("Crowbar Settings")]
    public float hpThreshold = 0.8f; // 80% 이상일 때
    public float bonusDamageRatio = 0.5f; // 중첩당 50% 추가 피해

    private void Awake()
    {
        tier = ItemTier.Common;
        maxStack = 0; // 무제한
    }

    public override void OnHitEnemy(PlayerController player, GameObject target, float damage, int stackCount)
    {
        if (target.TryGetComponent(out EnemyBase enemy))
        {
            float hpRatio = enemy.GetHpRatio();

            // 적의 체력이 80% 이상이라면?
            if (hpRatio >= hpThreshold)
            {
                float extraDamage = damage * bonusDamageRatio * stackCount;

                DamageInfo bonusInfo = new DamageInfo
                {
                    Amount = extraDamage,
                    HitPoint = target.transform.position,
                    HitDirection = player.IsFacingRight ? Vector2.right : Vector2.left,
                    KnockbackForce = 0f,
                    Attacker = player.gameObject,
                    IsCrit = false,
                    CanProc = false 
                };
                enemy.TakeDamage(bonusInfo);
                Debug.Log($"<color=orange>[Item] {itemName} 발동! 적에게 {extraDamage}의 쇠지랫대 추가 피해를 입혔습니다.</color>");
            }
        }
    }
}
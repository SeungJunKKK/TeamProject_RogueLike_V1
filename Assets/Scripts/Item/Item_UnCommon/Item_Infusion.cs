using UnityEngine;

[CreateAssetMenu(fileName = "Infusion", menuName = "Items/UnCommon/Infusion")]
public class Item_Infusion : ItemData
{
    public override void OnKillEnemy(PlayerController player, GameObject target, int stackCount)
    {
        // 1개면 1.0, 2개면 1.5, 3개면 2.0 증가
        float bonusHealth = 0.5f + (0.5f * stackCount);

        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.MaxHealth.BaseValue += bonusHealth;
            EventBus.Publish(new PlayerDamagedEvent
            {
                Amount = 0,
                CurrentHp = stats.CurrentHealth,
                MaxHp = stats.MaxHealth.Value
            });

            Debug.Log($"[Item] 최대 체력 {bonusHealth} 영구 증가 (현재 최대 체력: {stats.MaxHealth.Value})");
        }
    }
}
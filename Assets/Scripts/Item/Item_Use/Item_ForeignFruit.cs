using UnityEngine;

[CreateAssetMenu(fileName = "Item_ForeignFruit", menuName = "Items/Use/Foreign Fruit")]
public class Item_ForeignFruit : ItemData
{
    [Header("Foreign Fruit Settings")]
    public float healPercentage = 0.5f;

    [Header("Sound Settings")]
    public AudioClip healSound;

    [Header("VFX Settings")]
    public GameObject healVfxPrefab;

    private void Awake()
    {
        tier = ItemTier.Use; // 주황색(사용) 등급
        maxStack = 1;        // 장비는 1개만 들 수 있음
        cooldown = 40f;      // 쿨타임 40초
    }

    public override bool OnUse(PlayerController player)
    {
        if (player.TryGetComponent(out PlayerStats stats))
        {
            // 1. 이미 체력이 100%라면 사용 실패 처리 
            if (stats.CurrentHealth >= stats.MaxHealth.Value)
            {
                Debug.Log("<color=yellow>[시스템] 체력이 이미 가득 차 있어 이계의 과일을 사용할 수 없습니다.</color>");
                return false;
            }

            // 2. 강화(Upgraded) 상태 판별 로직
            bool isUpgraded = false;
            float actualHealPercentage = isUpgraded ? healPercentage * 2f : healPercentage; // 강화 시 2배(100%)

            float healAmount = stats.MaxHealth.Value * actualHealPercentage;
            stats.CurrentHealth += healAmount;

            if (stats.CurrentHealth > stats.MaxHealth.Value)
            {
                stats.CurrentHealth = stats.MaxHealth.Value;
            }

            Debug.Log($"<color=green>[이계의 과일 사용!] {healAmount:F1}만큼 체력을 회복했습니다. (현재 체력: {stats.CurrentHealth:F1}/{stats.MaxHealth.Value})</color>");

            if (healVfxPrefab != null)
            {
                // 생성할 십자가 개수
                int effectCount = Random.Range(5, 10);

                for (int i = 0; i < effectCount; i++)
                {
                    //Vector3 centerPos = player.transform.position + new Vector3(0f, -0.5f, 0f);

                    Vector3 centerPos= player.FeetPos.transform.position;


                    float randomX = Random.Range(-0.6f, 0.6f);
                    float randomY = Random.Range(-0.2f, 0.2f); 
                    Vector3 randomOffset = new Vector3(randomX, randomY, 0f);

                    Vector3 spawnPos = centerPos + randomOffset;

                    GameObject vfx = PoolManager.Instance.Get(healVfxPrefab, spawnPos, Quaternion.identity);

                    float randomScale = Random.Range(0.6f, 1.2f);
                    vfx.transform.localScale = new Vector3(randomScale, randomScale, 1f);

                    vfx.transform.SetParent(player.transform);
                }
            }

            if (healSound != null)
            {
                SoundManager.Instance.PlaySFX(healSound);
            }

            return true; 
        }

        return false;
    }
}
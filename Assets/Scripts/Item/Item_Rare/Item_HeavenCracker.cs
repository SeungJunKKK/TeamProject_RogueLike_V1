using UnityEngine;

[CreateAssetMenu(fileName = "Item_HeavenCracker", menuName = "Items/Rare/Heaven Cracker")]
public class Item_HeavenCracker : ItemData
{
    [Header("Heaven Cracker Settings")]
    public int baseRequiredAttacks = 4; // 기본 4타 요구
    public float damageMultiplier = 1.0f; // 100% 피해
    public float drillRange = 10f; // 드릴 관통 사거리
    public float drillThickness = 1.5f; // 드릴 타격 범위 두께
    [Header("VFX Settings")]
    public GameObject drillVfxPrefab;
    [Header("Sound Settings")]
    public AudioClip drillSound;


    private void Awake()
    {
        tier = ItemTier.Rare;
        maxStack = 4; 
    }

    public override void OnBasicAttack(PlayerController player, int stackCount)
    {
        if (!player.TryGetComponent(out HeavenCrackerTracker tracker))
        {
            tracker = player.gameObject.AddComponent<HeavenCrackerTracker>();
            Debug.Log("[디버그] 플레이어에게 HeavenCrackerTracker 컴포넌트를 부착했습니다.");
        }

        // 요구 타격 수 계산 (중첩당 -1, 최소 1타)
        // 1스택 = 4타, 2스택 = 3타, 3스택 = 2타, 4스택 = 1타(매번)
        int requiredAttacks = Mathf.Max(1, baseRequiredAttacks - (stackCount - 1));

        int currentCount = tracker.IncrementAndGetCount();
        //Debug.Log($"<color=orange>[디버그] 3. 천공 분쇄기 계산 -> 현재: {currentCount}타 / 요구치: {requiredAttacks}타 (현재 {stackCount}스택)</color>");
        // 발동 조건 달성 시
        if (currentCount >= requiredAttacks)
        {
            tracker.ResetCount(); // 카운트 초기화
            //Debug.Log("<color=red>[디버그] 4. 드릴 발사(FireDrill) 함수 호출!</color>");
            if (drillSound != null)
            {
                SoundManager.Instance.PlaySFX(drillSound);
            }
            FireDrill(player);   
        }
    }

    private void FireDrill(PlayerController player)
    {
        Vector2 origin = player.MuzzlePos.position;
        Vector2 direction = player.IsFacingRight ? Vector2.right : Vector2.left;

        //중심점 보정:
        Vector2 boxCenter = origin + (direction * (drillRange / 2f));
        Vector2 boxSize = new Vector2(drillRange, drillThickness);

        // ===== 디버그: 실제 판정 박스를 Scene 뷰에 1초간 그림 =====
        DrawDebugBox(boxCenter, boxSize, Color.red, 1f);

        RaycastHit2D[] hits = Physics2D.BoxCastAll(boxCenter, boxSize, 0f, Vector2.zero, 0f, LayerMask.GetMask("Enemy"));

        PlayerStats stats = player.GetComponent<PlayerStats>();
        float drillDamage = stats != null ? stats.Damage.Value * damageMultiplier : 10f;

        foreach (var hit in hits)
        {
            //Debug.Log($"<color=cyan>[디버그] 판정됨: {hit.collider.name} / pos: {hit.collider.bounds.center} / layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}</color>");

            if (hit.collider.TryGetComponent(out EnemyBase enemy))
            {
                DamageInfo drillInfo = new DamageInfo
                {
                    Amount = drillDamage,
                    HitPoint = hit.point,
                    HitDirection = direction,
                    KnockbackForce = 0.5f, 
                    Attacker = player.gameObject,
                    IsCrit = stats != null && stats.RollCriticalHit(),
                    CanProc = true
                };

                enemy.TakeDamage(drillInfo);
            }
        }

        //Debug.Log($"<color=yellow>[천공 분쇄기 발동!] {hits.Length}명의 적을 관통했습니다.</color>");

        if (drillVfxPrefab != null)
        {
            GameObject vfx = PoolManager.Instance.Get(drillVfxPrefab, origin, player.transform.rotation);

            //if (!player.IsFacingRight)
            //{
            //    vfx.transform.localScale = new Vector3(-1, 1, 1);
            //}
            //else
            //{
            //    vfx.transform.localScale = new Vector3(1, 1, 1);
            //}
        }
    }

    // ===== 디버그 전용: 임의의 박스를 Scene 뷰에 그려줌 =====
    private static void DrawDebugBox(Vector2 center, Vector2 size, Color color, float duration)
    {
        Vector2 half = size / 2f;
        Vector2 topLeft = center + new Vector2(-half.x, half.y);
        Vector2 topRight = center + new Vector2(half.x, half.y);
        Vector2 bottomLeft = center + new Vector2(-half.x, -half.y);
        Vector2 bottomRight = center + new Vector2(half.x, -half.y);

        Debug.DrawLine(topLeft, topRight, color, duration);
        Debug.DrawLine(topRight, bottomRight, color, duration);
        Debug.DrawLine(bottomRight, bottomLeft, color, duration);
        Debug.DrawLine(bottomLeft, topLeft, color, duration);
    }


}
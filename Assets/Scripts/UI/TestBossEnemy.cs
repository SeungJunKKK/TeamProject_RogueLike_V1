using UnityEngine;

public class TestBossEnemy : MonoBehaviour, IDamageable
{
    [Header("Boss Stats")]
    [SerializeField] private float maxHp = 1000f;
    private float currentHp;

    [Header("UI Reference")]
    [SerializeField] private BossHPBarUI bossHPBar;

    private void Start()
    {
        currentHp = maxHp;

        // Scene 내의 BossHPBar를 자동으로 찾거나 인스펙터 연결
        if (bossHPBar == null)
        {
            bossHPBar = FindAnyObjectByType<BossHPBarUI>();
        }

        if (bossHPBar != null)
        {
            bossHPBar.InitHPBar(maxHp);
        }
    }

    // 플레이어의 TakeDamage(DamageInfo) 인터페이스 대응
    public void TakeDamage(DamageInfo damageInfo)
    {
        currentHp -= damageInfo.Amount;
        Debug.Log($"[Boss Test] 피격! 받은 데미지: {damageInfo.Amount}, 남은 HP: {currentHp}");

        if (bossHPBar != null)
        {
            bossHPBar.UpdateHPBar(currentHp);
        }

        if (currentHp <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("[Boss Test] 보스 처치됨!");
        gameObject.SetActive(false);
    }
}
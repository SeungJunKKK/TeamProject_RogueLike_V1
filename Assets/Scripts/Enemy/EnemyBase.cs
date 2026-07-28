using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamageable, IPoolable
{
    [Header("Stats (레벨 1 기준)")]
    [SerializeField] protected float m_BaseHp = 80f;
    [SerializeField] protected float m_HpPerLevel = 24f;
    [SerializeField] protected int m_BaseGold = 2;

    protected float m_CurrentHp;
    private Rigidbody2D m_Rigidbody;

    // 자식 클래스(BasicEnemyAI 등)에서 반드시 구현해야 하는 추상 메서드
    public abstract void SetTarget(Transform target);

    public virtual void OnSpawn()
    {
        // 스폰 시점의 난이도로 최대 HP 결정 (레벨 기반 스케일링)
        m_CurrentHp = DifficultyManager.Instance.GetScaledStat(m_BaseHp, m_HpPerLevel);

        // m_Rigidbody가 null일 때만 GetComponent 실행 (성능 최적화)
        m_Rigidbody ??= GetComponent<Rigidbody2D>();
    }

    public virtual void OnDespawn()
    {
        StopAllCoroutines();
    }

    public void TakeDamage(DamageInfo info)
    {
        // 이미 죽은 몬스터 재타격 방어 (동시 피격 시 Die 중복 호출 → 골드 이중 지급 방지)
        if (m_CurrentHp <= 0f)
        {
            return;
        }

        m_CurrentHp -= info.Amount;

        if (m_Rigidbody != null)
        {
            m_Rigidbody.AddForce(info.HitDirection * info.KnockbackForce, ForceMode2D.Impulse);
        }

        EventBus.Publish(new MonsterDamagedEvent
        {
            Amount = info.Amount,
            HitPoint = info.HitPoint,
            IsCrit = info.IsCrit
        });

        if (m_CurrentHp <= 0f)
        {
            Die();
        }
    }

    // ===== 사망 처리: 보스는 override 해서 페이즈/BossDiedEvent 추가 =====
    protected virtual void Die()
    {
        int gold = Mathf.Max(1, (int)(m_BaseGold * DifficultyManager.Instance.GetGoldMultiplier()));

        EventBus.Publish(new MonsterDiedEvent
        {
            Gold = gold,
            Exp = gold * 0.5f,     // RoR: 경험치는 골드의 절반
            Position = transform.position
        });

        // 스포너에서 자신을 제거
        EnemySpawner.Instance.UnregisterEnemy(gameObject);

        // 오브젝트 풀로 반환
        GetComponent<PooledObject>().Return();
    }
}
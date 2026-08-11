using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamageable, IPoolable
{
    [Header("Stats (레벨 1 기준)")]
    [SerializeField] protected float m_BaseHp = 80f;
    [SerializeField] protected float m_HpPerLevel = 24f;
    [SerializeField] protected int m_BaseGold = 2;

    protected float m_MaxHp;
    protected float m_CurrentHp;
    protected Rigidbody2D m_Rigidbody;
    public float MaxHp => m_MaxHp;
    public float CurrentHp => m_CurrentHp;
    public float GetHpRatio()
    {
        return m_MaxHp > 0f ? m_CurrentHp / m_MaxHp : 0f;
    }


    // 자식 클래스
    public abstract void SetTarget(Transform target);

    /// <summary>
    /// 스폰 시점에 호출되는 초기화 함수 -> 난이도에 따른 HP 스케일링 적용 -> 아이템 적용을 위해 MaxHp와 CurrentHp를 설정 
    /// (item 에서 값을 확인할수 있도록 maxHP와 currentHP를 public으로 노출)
    /// </summary>
    public virtual void OnSpawn()
    {
        if (DifficultyManager.Instance != null)
        {
            // 스폰 시점의 난이도로 최대 HP 결정
            //m_CurrentHp = DifficultyManager.Instance.GetScaledStat(m_BaseHp, m_HpPerLevel);
            m_MaxHp = DifficultyManager.Instance.GetScaledStat(m_BaseHp, m_HpPerLevel);
        }
        else
        {
            m_MaxHp = m_BaseHp;
        }

        m_CurrentHp = m_MaxHp;
        // m_Rigidbody가 null일 때만 GetComponent 실행
        m_Rigidbody ??= GetComponent<Rigidbody2D>();
    }

    public virtual void OnDespawn()
    {
        StopAllCoroutines();
    }

    public void TakeDamage(DamageInfo info)
    {
        // 이미 죽은 몬스터 재타격 방어
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
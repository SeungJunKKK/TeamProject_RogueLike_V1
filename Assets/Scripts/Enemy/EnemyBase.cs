using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamageable, IPoolable
{
    [Header("적 데이터 (SO)")]
    [SerializeField] protected EnemyDataSO m_Data;

    [Header("사운드 최적화 (Audio Culling)")]
    [SerializeField] protected AudioSource m_AudioSource;
    [SerializeField] protected float m_MaxHearingDistance = 40f;

    //[Header("Stats (레벨 1 기준)")]
    //[SerializeField] protected float m_BaseHp = 80f;
    //[SerializeField] protected float m_HpPerLevel = 24f;
    //[SerializeField] protected int m_BaseGold = 2;

    //[SerializeField] protected float m_BaseExp = 12f;
    //[SerializeField] protected float m_ExpPerLevel = 3f;

    protected float m_MaxHp;
    protected float m_CurrentHp;
    protected Rigidbody2D m_Rigidbody;
    protected virtual bool CanReceiveKnockback => true;
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
            //m_MaxHp = DifficultyManager.Instance.GetScaledStat(m_BaseHp, m_HpPerLevel);
            m_MaxHp = DifficultyManager.Instance.GetScaledStat(m_Data.BaseHp, m_Data.HpPerLevel);
        }
        else
        {
            //m_MaxHp = m_BaseHp;
            m_MaxHp = m_Data.BaseHp;
        }

        m_CurrentHp = m_MaxHp;
        // m_Rigidbody가 null일 때만 GetComponent 실행
        m_Rigidbody ??= GetComponent<Rigidbody2D>();

        if(m_Data.SpawnSoundAddress != null)
        {
            PlayAddressableSFX(m_Data.SpawnSoundAddress);
        }

        // if(m_Data.SpawnSound != null) SoundManager.Play(m_Data.SpawnSound);
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

        if (CanReceiveKnockback && m_Rigidbody != null)
        {
            m_Rigidbody.AddForce(info.HitDirection * info.KnockbackForce, ForceMode2D.Impulse);
        }


        EventBus.Publish(new MonsterDamagedEvent
        {
            Amount = info.Amount,
            HitPoint = info.HitPoint,
            IsCrit = info.IsCrit
        });

        if(m_Data.HitSoundAddress != null)
        {
            PlayAddressableSFX(m_Data.HitSoundAddress);
        }


        if (m_CurrentHp <= 0f)
        {
            Die();
        }
    }

    // ===== 사망 처리: 보스는 override 해서 페이즈/BossDiedEvent 추가 =====
    protected virtual void Die()
    {
        //int gold = Mathf.Max(1, (int)(m_BaseGold * DifficultyManager.Instance.GetGoldMultiplier()));
        //float exp = DifficultyManager.Instance.GetScaledStat(m_BaseExp, m_ExpPerLevel);
        if(m_Data.DeathSoundAddress != null)
        {
            PlayAddressableSFX(m_Data.DeathSoundAddress);
        }

        int gold = Mathf.Max(1, (int)(m_Data.BaseGold * DifficultyManager.Instance.GetGoldMultiplier()));
        float exp = DifficultyManager.Instance.GetScaledStat(m_Data.BaseExp, m_Data.ExpPerLevel);

        EventBus.Publish(new MonsterDiedEvent
        {
            Gold = gold,
            Exp = exp,
            Position = transform.position
        });

        EnemySpawner.Instance.UnregisterEnemy(gameObject);
    }
    public void PlayAddressableSFX(string sfxAddress)
    {
        if (string.IsNullOrEmpty(sfxAddress))
        {
            return;
        }

        if (Camera.main != null)
        {
            float distance = Vector2.Distance(transform.position, Camera.main.transform.position);

            if (distance > m_MaxHearingDistance)
            {
                return;
            }
        }

        if (m_AudioSource == null)
        {
            m_AudioSource = GetComponent<AudioSource>();
        }

        AddressableManager.Instance.LoadAssetAsync<AudioClip>(sfxAddress, (clip) =>
        {
            if (clip != null)
            {
                if (m_AudioSource != null)
                {
                    m_AudioSource.PlayOneShot(clip);
                }
                else
                {
                    Debug.LogWarning($"[사운드 경고] {gameObject.name}에 AudioSource가 없어 소리를 낼 수 없습니다!");
                }
            }
            else
            {
                Debug.LogError($"[어드레서블 에러] '{sfxAddress}' 주소로 효과음을 찾을 수 없습니다!");
            }
        });
    }

}
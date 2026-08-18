using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class ShieldSlamState : IState
{
    private readonly PlayerController m_Player;
    private readonly float m_Duration;
    private float m_Timer;
    private bool m_HasSlammed;

    // 히트박스 설정
    private readonly Vector2 m_HitboxOffset = new Vector2(0f, 0.5f);

    public ShieldSlamState(PlayerController player, float duration)
    {
        m_Player = player;
        m_Duration = duration;
    }

    public void Enter()
    {
        m_Timer = 0f;
        m_HasSlammed = false;

        Debug.Log($"<color=magenta>[디버그] AttackSpeed: {m_Player.Stats.AttackSpeed.Value} / Duration: {m_Duration} / Anim null? {m_Player.Anim == null}</color>");

        if (m_Player.Rb != null)
        {
            m_Player.Rb.linearVelocity = new Vector2(0f, m_Player.Rb.linearVelocity.y);
        }

        if (m_Player.Anim != null)
        {
            m_Player.Anim.speed = m_Player.Stats.AttackSpeed.Value;
            m_Player.Anim.Play("X_Shield_Slam");

        }

        Debug.Log("<color=cyan>[Player] 방패 밀치기(Shield Slam) 진입</color>");
    }

    public void Update()
    {
        m_Timer += Time.deltaTime;

        if (!m_HasSlammed && m_Timer >= m_Duration * 0.3f)
        {
            m_HasSlammed = true;
            ExecuteSlam();
        }

        if (m_Timer >= m_Duration)
        {
            m_Player.ChangeState(new PlayerIdleState(m_Player));
        }
    }

    public void Exit()
    {
        if (m_Player.Anim != null)
        {
            m_Player.Anim.speed = 1.0f;
        }
    }

    private void ExecuteSlam()
    {
        Vector2 slamDirection = m_Player.IsFacingRight ? Vector2.right : Vector2.left;

        // 방패 밀치기 광역 판정 (등 뒤와 머리 위까지 커버하는 OverlapBoxAll)
        Vector2 center = (Vector2)m_Player.transform.position + m_HitboxOffset;
        Vector2 boxSize = new Vector2(2.5f, 2.0f);

        LayerMask enemyLayer = LayerMask.GetMask("Enemy", "FlyingEnemy");
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, boxSize, 0f, enemyLayer);

        bool hitSomething = false;
        bool isCrit = m_Player.Stats.RollCriticalHit();

        float baseDamage = m_Player.Stats.Damage.Value * 2.1f;
        float finalDamage = isCrit ? baseDamage * m_Player.Stats.CritDamage.Value : baseDamage;

        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                DamageInfo info = new DamageInfo
                {
                    Amount = finalDamage,
                    HitPoint = (Vector2)hit.transform.position,
                    HitDirection = slamDirection,
                    KnockbackForce = 1f,
                    Attacker = m_Player.gameObject,
                    IsCrit = isCrit,
                    CanProc = true
                };

                damageable.TakeDamage(info);
                hitSomething = true;

                if (m_Player.ShieldSlamDustPrefab != null)
                {
                    PoolManager.Instance.Get(m_Player.ShieldSlamDustPrefab, hit.transform.position, Quaternion.identity);
                }

                if (info.CanProc)
                {
                    m_Player.OnEnemyHit(hit.gameObject, finalDamage);
                }

                if (hit.TryGetComponent<Rigidbody2D>(out var enemyRb))
                {
                    Vector2 knockbackVector = new Vector2(slamDirection.x * 1f, 5f);
                    enemyRb.linearVelocity = knockbackVector;
                }
            }
        }

        m_Player.PlayAddressableSFX(m_Player.X_SFXAddress);

        if (hitSomething)
        {
            m_Player.TriggerHitFeedback(slamDirection, 0.015f, 0.01f);
        }
    }
}
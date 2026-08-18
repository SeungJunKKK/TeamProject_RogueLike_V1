using UnityEngine;

public class EnforcerShotgunState : IState
{
    private EnforcerController m_Player;
    private float m_Duration = 0.4f; 
    private float m_Timer;

    public EnforcerShotgunState(EnforcerController player, float duration)
    {
        m_Player = player;
        m_Duration = duration;  
    }

    public void Enter()
    {
        m_Timer = 0f;
        m_Player.Rb.linearVelocity = new Vector2(0f, m_Player.Rb.linearVelocity.y);

        float attackSpeed = Mathf.Max(0.1f, m_Player.Stats.AttackSpeed.Value);
        m_Player.Anim.speed = attackSpeed;

        if (m_Player.IsDefending)
        {
            m_Player.Anim.Play("Enforcer_Shield_Attack");
        }
        else
        {
            m_Player.Anim.Play("Enforcer_Attack");
        }
        m_Player.CooldownManager.UseSkill(SkillType.Primary_Z);
    }

    public void FireShotgun()
    {
        Vector2 shootDirection = m_Player.IsFacingRight ? Vector2.right : Vector2.left;
        bool isShootingRight = shootDirection.x > 0;
        m_Player.SpawnDustEffect(isShootingRight, EDustType.Recoil);

        Vector2 shootOrigin = m_Player.MuzzlePos != null
                              ? (Vector2)m_Player.MuzzlePos.position
                              : (Vector2)m_Player.transform.position + new Vector2(shootDirection.x * 0.5f, 0.2f);

        Vector2 boxSize = new Vector2(3f, 2.5f);
        float attackRange = 1.5f;

        int hitLayerMask = LayerMask.GetMask("Enemy", "FlyingEnemy", "Ground", "OneWayGround", "Wall", "Default");
        RaycastHit2D[] hits = Physics2D.BoxCastAll(shootOrigin, boxSize, 0f, shootDirection, attackRange, hitLayerMask);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        bool hitSomething = false;
        bool isCrit = m_Player.Stats.RollCriticalHit();
        float baseDamage = m_Player.Stats.Damage.Value * 1.6f;
        float finalDamage = isCrit ? baseDamage * m_Player.Stats.CritDamage.Value : baseDamage;

        foreach (RaycastHit2D hit in hits)
        {
            int hitLayer = hit.collider.gameObject.layer;

            if (hitLayer == LayerMask.NameToLayer("Ground") || hitLayer == LayerMask.NameToLayer("Wall") ||
                hitLayer == LayerMask.NameToLayer("Default") || hitLayer == LayerMask.NameToLayer("OneWayGround"))
            {
                break;
            }

            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                DamageInfo info = new DamageInfo
                {
                    Amount = finalDamage,
                    HitPoint = hit.point != Vector2.zero ? hit.point : (Vector2)hit.collider.transform.position,
                    HitDirection = shootDirection,
                    KnockbackForce = 0.5f,
                    Attacker = m_Player.gameObject,
                    IsCrit = isCrit,
                    CanProc = true
                };

                damageable.TakeDamage(info);
                hitSomething = true;
                if (info.CanProc)
                {
                    m_Player.OnEnemyHit(hit.collider.gameObject, finalDamage);
                }
            }
        }

        m_Player.PlayAddressableSFX(m_Player.Z_SFXAddress);
        if (hitSomething)
        {
            m_Player.TriggerHitFeedback(shootDirection, 0.01f, 0f);
        }
        else
        {
         m_Player.TriggerHitFeedback(shootDirection, 0.005f, 0f);
        }
    }

    public void Update()
    {
        m_Timer += Time.deltaTime;

        if (m_Timer >= m_Duration)
        {
            m_Player.ChangeState(new PlayerIdleState(m_Player));
        }
    }

    public void Exit()
    {
        m_Player.Anim.speed = 1f;
    
    }
}
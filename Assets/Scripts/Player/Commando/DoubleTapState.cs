using UnityEngine;


namespace Player.Commando
{
public class DoubleTapState : PlayerAttackState
{
    private readonly float m_TotalDuration;
    private bool m_HasFiredSecondShot;

    public DoubleTapState(PlayerController player, float duration)
        : base(player, "Z_DoubleTap", duration,SkillType.Primary_Z) 
    {
        m_TotalDuration = duration;
        m_HasFiredSecondShot = false;
    }

    public override void Update()
    {
        base.Update();

        if (!m_HasFiredSecondShot && m_AttackTimer <= m_TotalDuration / 2f)
        {
            ExecuteShoot();
            m_HasFiredSecondShot = true;
        }
    }
        protected override void ExecuteShoot()
        {
            Vector2 shootDirection = m_Player.transform.right;

            Vector2 shootOrigin = m_Player.MuzzlePos != null
                                  ? (Vector2)m_Player.MuzzlePos.position
                                  : (Vector2)m_Player.transform.position + new Vector2(shootDirection.x * 0.5f, 0.2f);

            if (m_Player.DoubleTapProjectilePrefab != null)
            {
                DamageInfo attackInfo = new DamageInfo
                {
                    Amount = 10f,
                    HitDirection = shootDirection,
                    KnockbackForce = 15f,
                    Attacker = m_Player.gameObject,
                    IsCrit = false,
                    CanProc = true
                };
                EventBus.Publish(new SpawnProjectileEvent
                {
                    ProjectilePrefab = m_Player.DoubleTapProjectilePrefab,
                    Position = shootOrigin,
                    Rotation = m_Player.transform.rotation,
                    Direction = shootDirection,
                    Speed = 20f,
                    IsPiercing = false,

                    AttackData = attackInfo 
                });
            }

            m_Player.TriggerHitFeedback(shootDirection, 0.3f, 0.05f);
        }
    }
}
using UnityEngine;

namespace Player.Commando
{
    public class FullMetalJacketState : PlayerAttackState
    {
        public FullMetalJacketState(PlayerController player, float duration)
            : base(player, "X_FullMetalJacket", duration, SkillType.Secondary_X) { }

        protected override void ExecuteShoot()
        {

            Vector2 shootDirection = m_Player.transform.right;
            bool isShootingRight = shootDirection.x > 0;
            m_Player.SpawnDustEffect(isShootingRight, EDustType.Recoil);

            Vector2 shootOrigin = m_Player.MuzzlePos != null
                                  ? (Vector2)m_Player.MuzzlePos.position
                                  : (Vector2)m_Player.transform.position + new Vector2(shootDirection.x * 0.5f, 0.2f);

            float attackRange = 30f;

            RaycastHit2D[] hits = Physics2D.RaycastAll(shootOrigin, shootDirection, attackRange);

            bool hitSomething = false;
            bool isCrit = m_Player.Stats.RollCriticalHit();
            float baseDamage = m_Player.Stats.Damage.Value * 2.3f; 
            float finalDamage = isCrit ? baseDamage * m_Player.Stats.CritDamage.Value : baseDamage;

            foreach (RaycastHit2D hit in hits)
            {
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    DamageInfo info = new DamageInfo
                    {
                        Amount = finalDamage,
                        HitPoint = hit.point,
                        HitDirection = shootDirection,
                        KnockbackForce = 0.1f, 
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
            m_Player.PlayAddressableSFX(m_Player.X_SFXAddress);


            if (hitSomething)
            {
                m_Player.TriggerHitFeedback(shootDirection, 0.6f, 0f);
            }
            else
            {
                m_Player.TriggerHitFeedback(shootDirection, 0.2f, 0f);
            }
        }
    }
}
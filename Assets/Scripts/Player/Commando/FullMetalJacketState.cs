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

            Vector2 shootOrigin = m_Player.MuzzlePos != null
                                  ? (Vector2)m_Player.MuzzlePos.position
                                  : (Vector2)m_Player.transform.position + new Vector2(shootDirection.x * 0.5f, 0.2f);

            float attackRange = 15f;
            RaycastHit2D[] hits = Physics2D.RaycastAll(shootOrigin, shootDirection, attackRange);

            bool hitSomething = false;

            foreach (RaycastHit2D hit in hits)
            {
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    DamageInfo info = new DamageInfo
                    {
                        Amount = 25f,
                        HitPoint = hit.point,
                        HitDirection = shootDirection,
                        KnockbackForce = 25f,
                        Attacker = m_Player.gameObject,
                        IsCrit = false,
                        CanProc = true
                    };

                    damageable.TakeDamage(info);
                    hitSomething = true;
                }
            }

            Vector2 vfxPosition = shootOrigin + (shootDirection * (attackRange / 2f));

            if (m_Player.FullMetalJacketPrefab != null)
            {
                EventBus.Publish(new SpawnVFXEvent
                {
                    VFXPrefab = m_Player.FullMetalJacketPrefab,
                    Position = vfxPosition,
                    Rotation = m_Player.transform.rotation
                });
            }

            if (hitSomething)
                m_Player.TriggerHitFeedback(shootDirection, 0.6f, 0.1f);
            else
                m_Player.TriggerHitFeedback(shootDirection, 0.2f, 0f);
        }
    }
}
using UnityEngine;

namespace Player.Commando
{
    public class SuppressiveBarrageState : PlayerAttackState
    {
        private int m_CurrentShotCount = 0;
        private bool m_IsSingleMode;

        public SuppressiveBarrageState(PlayerController player, float duration)
            : base(player, "", duration, SkillType.Ultimate_V) { }

        public override void Enter()
        {
            base.Enter();
            m_Player.Rb.linearVelocity = Vector2.zero;
            m_Player.CooldownManager.UseSkill(m_SkillType);

            m_IsSingleMode = m_Player.MovementInput.x != 0;

            if (m_IsSingleMode)
            {
                float dir = Mathf.Sign(m_Player.MovementInput.x);
                m_Player.transform.rotation = Quaternion.Euler(0, dir > 0 ? 0 : 180, 0);
                m_Player.Anim.Play("SuppressiveBarrage_Single");
            }
            else
            {
                m_Player.Anim.Play("SuppressiveBarrage_Both");
            }
        }

        protected override void ExecuteShoot()
        {
            

            m_CurrentShotCount++;
            Vector2 shootDirection;
            Vector2 shootOrigin;

            if (m_IsSingleMode)
            {
                shootDirection = m_Player.transform.right;
                bool isShootingRight = shootDirection.x > 0;
                m_Player.SpawnDustEffect(isShootingRight, EDustType.Recoil);

                shootOrigin = m_Player.MuzzlePos != null
                              ? (Vector2)m_Player.MuzzlePos.position
                              : (Vector2)m_Player.transform.position + new Vector2(shootDirection.x * 0.5f, 0.2f);
            }
            else
            {
                float dir = (m_CurrentShotCount % 2 == 1) ? 1f : -1f;
                shootDirection = new Vector2(dir, 0f);
                bool isShootingRight = shootDirection.x > 0;
                m_Player.SpawnDustEffect(isShootingRight, EDustType.Recoil);

                if (m_Player.MuzzlePos != null)
                {
                    Vector3 localPos = m_Player.MuzzlePos.localPosition;
                    Vector3 mirroredOffset = new Vector3(Mathf.Abs(localPos.x) * dir, localPos.y, localPos.z);
                    shootOrigin = m_Player.transform.position + mirroredOffset;
                }
                else
                {
                    shootOrigin = (Vector2)m_Player.transform.position + new Vector2(dir * 0.5f, 0.2f);
                }
            }

            float attackRange = 20f; // 더 긴 사거리[cite: 14]
            RaycastHit2D[] hits = Physics2D.RaycastAll(shootOrigin, shootDirection, attackRange);
            bool hitSomething = false;

            bool isCrit = m_Player.Stats.RollCriticalHit();
            float baseDamage = m_Player.Stats.Damage.Value * 5.0f; // 강력한 데미지[cite: 14]
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
                        KnockbackForce = 3f,
                        Attacker = m_Player.gameObject,
                        IsCrit = isCrit,
                        CanProc = true
                    };

                    damageable.TakeDamage(info);
                    hitSomething = true;
                }
            }

            if (hitSomething)
            {
                m_Player.TriggerHitFeedback(shootDirection, 0.7f, 0.05f);
            }
            else
            {
                m_Player.TriggerHitFeedback(shootDirection, 0.4f, 0f);
            }

            m_Player.PlayAddressableSFX(m_Player.VStrengthened_SFXAddress);

        }
    }
}
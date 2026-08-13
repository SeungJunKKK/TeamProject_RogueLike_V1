using UnityEngine;

namespace Player.Commando
{
    public class DoubleTapState : PlayerAttackState
    {
        private readonly float m_TotalDuration;

        public DoubleTapState(PlayerController player, float duration)
            : base(player, "Z_DoubleTap", duration, SkillType.Primary_Z)
        {
            m_TotalDuration = duration;
        }


        protected override void ExecuteShoot()
        {
            m_Player.PlayAddressableSFX(m_Player.Z_SFXAddress);

            Vector2 shootDirection = m_Player.transform.right;
            Vector2 shootOrigin = m_Player.MuzzlePos != null
                                  ? (Vector2)m_Player.MuzzlePos.position
                                  : (Vector2)m_Player.transform.position + new Vector2(shootDirection.x * 0.5f, 0.2f);

            float attackRange = 30f;
            int enemyLayer = LayerMask.GetMask("Enemy", "FlyingEnemy");
            RaycastHit2D hit = Physics2D.Raycast(shootOrigin, shootDirection, attackRange, enemyLayer);

            if (hit.collider != null)
            {
                // 적에게 맞았을 경우: 발사 위치부터 적중한 지점까지 빨간색 선 표시
                Debug.DrawLine(shootOrigin, hit.point, Color.red, 1.0f);
            }
            else
            {
                // 허공에 쐈을 경우: 발사 위치부터 최대 사거리까지 초록색 선 표시
                Debug.DrawRay(shootOrigin, shootDirection * attackRange, Color.green, 1.0f);
            }

            bool hitSomething = false;
            bool isCrit = m_Player.Stats.RollCriticalHit();
            float baseDamage = m_Player.Stats.Damage.Value * 0.6f;
            float finalDamage = isCrit ? baseDamage * m_Player.Stats.CritDamage.Value : baseDamage;

            if (hit.collider != null)
            {
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    DamageInfo info = new DamageInfo
                    {
                        Amount = finalDamage,
                        HitPoint = hit.point,
                        HitDirection = shootDirection,
                        KnockbackForce = 1f,
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

            if (hitSomething)
            {
                m_Player.TriggerHitFeedback(shootDirection, 0.3f, 0.05f);
                m_Player.TriggerHitStop(0.05f);
            }
            else
            {
                m_Player.TriggerHitFeedback(shootDirection, 0.1f, 0f);
            }
        }
    }
}
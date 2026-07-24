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
                TestDummyHealth dummy = hit.collider.GetComponent<TestDummyHealth>();
                if (dummy != null)
                {
                    dummy.TakeDamage(25f, shootDirection, 25f);
                    hitSomething = true;
                }
            }

            // 💡 3. 레이저의 중심점 보정 (총구 위치에서 7.5만큼 앞으로!)
            Vector2 vfxPosition = shootOrigin + (shootDirection * (attackRange / 2f));

            if (m_Player.FullMetalJacketPrefab != null)
            {
                EventBus.Publish(new SpawnVFXEvent
                {
                    VFXPrefab = m_Player.FullMetalJacketPrefab,
                    Position = vfxPosition,
                    // 💡 4. 플레이어의 각도를 그대로 복사해서 레이저에 적용하면 끝!
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
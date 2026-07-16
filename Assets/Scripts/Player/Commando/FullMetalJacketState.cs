using UnityEngine;


namespace Player.Commando
{
public class FullMetalJacketState : PlayerAttackState
{
    public FullMetalJacketState(PlayerController player, float duration)
        : base(player, "X_FullMetalJacket", duration,SkillType.Secondary_X)
    {
    }

    protected override void ExecuteShoot()
    {
        float facingDir = m_Player.IsFacingRight ? 1f : -1f;
        Vector2 shootDirection = new Vector2(facingDir, 0f);
        Vector2 shootOrigin = new Vector2(m_Player.transform.position.x, m_Player.transform.position.y + 0.2f);

        Debug.DrawRay(shootOrigin, shootDirection * 10f, Color.red, 1f);

        RaycastHit2D[] hits = Physics2D.RaycastAll(shootOrigin, shootDirection, 10f);

        foreach (RaycastHit2D hit in hits)
        {
            TestDummyHealth dummy = hit.collider.GetComponent<TestDummyHealth>();
            if (dummy != null)
            {
                dummy.TakeDamage(25f, shootDirection, 2f);
                m_Player.TriggerHitFeedback(shootDirection, 0.3f, 0.05f);
            }
        }
    }
}

}
using UnityEngine;


namespace Player.Commando
{
public class DoubleTapState : PlayerAttackState
{
    private readonly float m_TotalDuration;
    private bool m_HasFiredSecondShot;

    public DoubleTapState(PlayerController player, float duration)
        : base(player, "Z_DoubleTap", duration) 
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
        float facingDir = m_Player.IsFacingRight ? 1f : -1f;
        Vector2 shootDirection = new Vector2(facingDir, 0f);
        Vector2 shootOrigin = new Vector2(m_Player.transform.position.x, m_Player.transform.position.y + 0.5f);

        Debug.DrawRay(shootOrigin, shootDirection * 10f, Color.red, 1f);

        RaycastHit2D[] hits = Physics2D.RaycastAll(shootOrigin, shootDirection, 10f);

        foreach (RaycastHit2D hit in hits)
        {
            DummyHealth dummy = hit.collider.GetComponent<DummyHealth>();
            if (dummy != null)
            {
                dummy.TakeDamage(10f); 
                break;
            }
        }
    }
}


}
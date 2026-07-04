using UnityEngine;

public class PlayerDashState : IState
{
    private PlayerController m_Player;
    private float m_DashTimer;
    private Vector2 m_DashDirection;

    public PlayerDashState(PlayerController player) { this.m_Player = player; }

    public void Enter()
    {
        m_Player.SpawnDustEffect(m_Player.IsFacingRight);
        m_Player.Anim.Play("Dash");

        m_DashTimer = m_Player.DashDuration;
        float dirX = m_Player.MovementInput.x != 0 ? Mathf.Sign(m_Player.MovementInput.x) : (m_Player.IsFacingRight ? 1f : -1f);
        m_DashDirection = new Vector2(dirX, 0f);

        m_Player.Rb.linearVelocity = m_DashDirection * m_Player.DashSpeed;
        m_Player.Rb.gravityScale = 0f;
    }

    public void Update()
    {
        m_DashTimer -= Time.deltaTime;

        if (m_DashTimer <= 0)
        {
            if (m_Player.MovementInput.x != 0) m_Player.ChangeState(new PlayerRunState(m_Player));
            else m_Player.ChangeState(new PlayerIdleState(m_Player));
        }
    }

    public void Exit()
    {
        m_Player.Rb.gravityScale = 3f;
        m_Player.Rb.linearVelocity = Vector2.zero;
    }
}
using UnityEngine;

public class PlayerJumpState : IState
{
    private PlayerController m_Player;

    public PlayerJumpState(PlayerController player) { this.m_Player = player; }

    public void Enter()
    {
        m_Player.SpawnDustEffect(m_Player.IsFacingRight);
        m_Player.Anim.Play("Jump");
        m_Player.Rb.linearVelocity = new Vector2(m_Player.Rb.linearVelocity.x, m_Player.JumpForce);
    }

    public void Update()
    {
        m_Player.Rb.linearVelocity = new Vector2(m_Player.MovementInput.x * m_Player.MoveSpeed, m_Player.Rb.linearVelocity.y);

        if (m_Player.Rb.linearVelocity.y <= 0.01f && m_Player.Rb.linearVelocity.y >= -0.01f)
        {
            if (m_Player.MovementInput.x != 0) m_Player.ChangeState(new PlayerRunState(m_Player));
            else m_Player.ChangeState(new PlayerIdleState(m_Player));
        }
    }

    public void Exit() { }
}
using UnityEngine;

public class PlayerFallState : IState
{
    private PlayerController m_Player;

    public PlayerFallState(PlayerController player) { this.m_Player = player; }

    public void Enter()
    {
        m_Player.Anim.Play("Jump");
    }

    public void Update()
    {
        m_Player.Rb.linearVelocity = new Vector2(m_Player.MovementInput.x * m_Player.Stats.MoveSpeed.Value, m_Player.Rb.linearVelocity.y);

        float verticalInput = Input.GetAxisRaw("Vertical");
        if (verticalInput > 0.1f && m_Player.CheckLadderUp() != null)
        {
            m_Player.ChangeState(new PlayerClimbState(m_Player, m_Player.CheckLadderUp()));
            return;
        }
        else if (verticalInput < -0.1f && m_Player.CheckLadderDown() != null)
        {
            m_Player.ChangeState(new PlayerClimbState(m_Player, m_Player.CheckLadderDown()));
            return;
        }

        if (m_Player.Rb.linearVelocity.y <= 0.01f && m_Player.Rb.linearVelocity.y >= -0.01f)
        {
            if (m_Player.MovementInput.x != 0) m_Player.ChangeState(new PlayerWalkState(m_Player));
            else m_Player.ChangeState(new PlayerIdleState(m_Player));
            return;
        }

        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.C)) && m_Player.CooldownManager.IsSkillReady(SkillType.Utility_C))
        {
            m_Player.ChangeState(new PlayerDashState(m_Player));
            return;
        }
    }

    public void Exit() { }
}
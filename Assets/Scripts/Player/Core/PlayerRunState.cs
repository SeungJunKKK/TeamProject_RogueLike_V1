using UnityEngine;

public class PlayerRunState : IState
{
    private readonly PlayerController m_Player; 

    public PlayerRunState(PlayerController player)
    {
        this.m_Player = player;
    }

    public void Enter()
    {
        m_Player.Anim.Play("Walk");
    }

    public void Update()
    {
        m_Player.Rb.linearVelocity = new Vector2(m_Player.MovementInput.x * m_Player.MoveSpeed, m_Player.Rb.linearVelocity.y);

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            m_Player.ChangeState(new PlayerDashState(m_Player));
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_Player.ChangeState(new PlayerJumpState(m_Player));
            return;
        }

        if (m_Player.MovementInput.x == 0)
        {
            m_Player.ChangeState(new PlayerIdleState(m_Player));
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            m_Player.ChangeState(new Player.Commando.DoubleTapState(m_Player, 0.4f));
            return;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            m_Player.ChangeState(new Player.Commando.FullMetalJacketState(m_Player, 0.2f));
            return;
        }
    }

    public void Exit() { }
}
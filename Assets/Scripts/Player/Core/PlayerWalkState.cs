using UnityEngine;

public class PlayerWalkState : IState
{
    private readonly PlayerController m_Player;

    public PlayerWalkState(PlayerController player)
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

        if (Input.GetKeyDown(KeyCode.C))
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
            IState attackState = m_Player.GetPrimaryAttackState();
            if (attackState != null)
            {
                m_Player.ChangeState(attackState);
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            IState attackState = m_Player.GetSecondaryAttackState();
            if (attackState != null)
            {
                m_Player.ChangeState(attackState);
                return;
            }
        }
    }

    public void Exit() { }
}
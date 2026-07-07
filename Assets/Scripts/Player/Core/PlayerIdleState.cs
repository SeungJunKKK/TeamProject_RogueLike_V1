using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class PlayerIdleState : IState
{
    private PlayerController m_Player;

    public PlayerIdleState(PlayerController player)
    {
        this.m_Player = player;
    }

    public void Enter()
    {
        //Debug.Log("시스템: 대기(Idle) 상태 진입 완료.");

         m_Player.Anim.Play("Idle"); 

        m_Player.Rb.linearVelocity = Vector2.zero;
    }

    public void Update()
    {
        if(m_Player.MovementInput.sqrMagnitude > 0.01f)
        {
            m_Player.ChangeState(new PlayerWalkState(m_Player));
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_Player.ChangeState(new PlayerJumpState(m_Player));
            return;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            m_Player.ChangeState(new PlayerDashState(m_Player));
            return;
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

    public void Exit()
    {
        //Debug.Log("시스템: 대기(Idle) 상태 해제.");
    }
}
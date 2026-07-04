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
            m_Player.ChangeState(new PlayerRunState(m_Player));
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

    public void Exit()
    {
        //Debug.Log("시스템: 대기(Idle) 상태 해제.");
    }
}
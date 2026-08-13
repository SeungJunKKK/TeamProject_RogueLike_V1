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
       // Debug.Log($"<color=orange>[PlayerIdleState 진입] 현재 방어 중?: {m_Player.IsDefending}</color>");
        if (m_Player.IsDefending)
        {
            m_Player.Anim.Play("Shield_Idle"); 
        }
        else
        {
            m_Player.Anim.Play("Idle"); 
        }
        m_Player.Rb.linearVelocity = Vector2.zero;
    }

    public void Update()
    {
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

        if (m_Player.MovementInput.sqrMagnitude > 0.01f)
        {
            m_Player.ChangeState(new PlayerWalkState(m_Player));
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_Player.ChangeState(new PlayerJumpState(m_Player));
            return;
        }
        // Z 스킬
        if (Input.GetKeyDown(KeyCode.Z) && m_Player.CooldownManager.IsSkillReady(SkillType.Primary_Z))
        {
            IState state = m_Player.GetPrimaryAttackState();
            if (state != null) m_Player.ChangeState(state);
            return;
        }
        // X 스킬
        if (Input.GetKeyDown(KeyCode.X) && m_Player.CooldownManager.IsSkillReady(SkillType.Secondary_X))
        {
            IState state = m_Player.GetSecondaryAttackState();
            if (state != null) m_Player.ChangeState(state);
            return;
        }

        // C 스킬 
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.C))
             && m_Player.CooldownManager.IsSkillReady(SkillType.Utility_C))
        {
            IState state = m_Player.GetUtilitySkillState();
            if (state != null)
            {
                m_Player.ChangeState(state);
            }
            return;
        }

        // V 스킬 
        if (Input.GetKeyDown(KeyCode.V) && m_Player.CooldownManager.IsSkillReady(SkillType.Ultimate_V))
        {
            IState state = m_Player.GetUltimateSkillState();
            if (state != null) m_Player.ChangeState(state);
            return;
        }
    }

    public void Exit()
    {
        //Debug.Log("시스템: 대기(Idle) 상태 해제.");
    }
}
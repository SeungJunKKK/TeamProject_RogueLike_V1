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
        if (m_Player.IsDefending)
        {
            m_Player.Anim.Play("Shield_Walk");
        }
        else
        {
            m_Player.Anim.Play("Walk");
        }
    }

    public void Update()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");
        float currentInput = m_Player.MovementInput.x;
        float targetSpeed = 0f;

        // ==========================================
        //  1. 점프 & 사다리 제한
        // ==========================================
        if (!m_Player.IsDefending)
        {
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

            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_Player.ChangeState(new PlayerJumpState(m_Player));
                return;
            }
        }

        // ==========================================
        // 2. 이동 속도 및 방향 제한 계산
        // ==========================================
        if (m_Player.IsDefending)
        {
            bool isPressingForward = (m_Player.IsFacingRight && currentInput > 0) || (!m_Player.IsFacingRight && currentInput < 0);

            if (isPressingForward)
            {
                targetSpeed = currentInput * m_Player.Stats.MoveSpeed.Value;
            }
            
        }
        else
        {
            // 일반 모드: 기존처럼 자유롭게 이동
            targetSpeed = currentInput * m_Player.Stats.MoveSpeed.Value;
        }

        // ==========================================
        //  3. 실제 물리 이동 적용 및 Idle 상태 전환
        // ==========================================
        m_Player.Rb.linearVelocity = new Vector2(targetSpeed, m_Player.Rb.linearVelocity.y);

        if (targetSpeed == 0f)
        {
            m_Player.ChangeState(new PlayerIdleState(m_Player));
            return;
        }
        // ==========================================
        //  4. 스킬 처리 
        // ==========================================
        if (Input.GetKeyDown(KeyCode.Z) && m_Player.CooldownManager.IsSkillReady(SkillType.Primary_Z))
        {
            IState state = m_Player.GetPrimaryAttackState();
            if (state != null) m_Player.ChangeState(state);
            return;
        }

        if (Input.GetKeyDown(KeyCode.X) && m_Player.CooldownManager.IsSkillReady(SkillType.Secondary_X))
        {
            IState state = m_Player.GetSecondaryAttackState();
            if (state != null) m_Player.ChangeState(state);
            return;
        }

        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.C))
             && m_Player.CooldownManager.IsSkillReady(SkillType.Utility_C))
        {
            IState state = m_Player.GetUtilitySkillState();
            if (state != null) m_Player.ChangeState(state);
            return;
        }

        if (Input.GetKeyDown(KeyCode.V) && m_Player.CooldownManager.IsSkillReady(SkillType.Ultimate_V))
        {
            IState state = m_Player.GetUltimateSkillState();
            if (state != null) m_Player.ChangeState(state);
            return;
        }
    }

    public void Exit() { }
}
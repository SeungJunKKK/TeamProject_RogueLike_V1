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

        m_Player.Rb.linearVelocity = new Vector2(m_Player.MovementInput.x * m_Player.Stats.MoveSpeed.Value, m_Player.Rb.linearVelocity.y);

        if (m_Player.MovementInput.x == 0)
        {
            m_Player.ChangeState(new PlayerIdleState(m_Player));
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_Player.ChangeState(new PlayerJumpState(m_Player));
            return;
        }

        // Z 스킬 (평타)
        if (Input.GetKeyDown(KeyCode.Z) && m_Player.CooldownManager.IsSkillReady(SkillType.Primary_Z))
        {
            IState state = m_Player.GetPrimaryAttackState();
            if (state != null) m_Player.ChangeState(state);
            return;
        }

        // X 스킬 (강공격)
        if (Input.GetKeyDown(KeyCode.X) && m_Player.CooldownManager.IsSkillReady(SkillType.Secondary_X))
        {
            IState state = m_Player.GetSecondaryAttackState();
            if (state != null) m_Player.ChangeState(state);
            return;
        }

        // C 스킬 (유틸기)
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.C))
             && m_Player.CooldownManager.IsSkillReady(SkillType.Utility_C))
        {
            m_Player.ChangeState(new PlayerDashState(m_Player));
            return;
        }

        // V 스킬 (궁극기)
        if (Input.GetKeyDown(KeyCode.V) && m_Player.CooldownManager.IsSkillReady(SkillType.Ultimate_V))
        {
            IState state = m_Player.GetUltimateSkillState();
            if (state != null) m_Player.ChangeState(state);
            return;
        }
    }

    public void Exit() { }
}
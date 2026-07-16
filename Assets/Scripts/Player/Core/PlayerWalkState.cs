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

        if (m_Player.MovementInput.x == 0)
        {
            m_Player.ChangeState(new PlayerIdleState(m_Player));
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
        if (Input.GetKeyDown(KeyCode.C) && m_Player.CooldownManager.IsSkillReady(SkillType.Utility_C))
        {
            IState state = m_Player.GetUtilitySkillState();
            if (state != null) m_Player.ChangeState(state);
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
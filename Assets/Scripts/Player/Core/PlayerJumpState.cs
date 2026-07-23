using UnityEngine;

public class PlayerJumpState : IState
{
    private PlayerController m_Player;

    public PlayerJumpState(PlayerController player) { this.m_Player = player; }

    public void Enter()
    {
        m_Player.SpawnDustEffect(m_Player.IsFacingRight, EDustType.Jump); 
        m_Player.Anim.Play("Jump");
        m_Player.Rb.linearVelocity = new Vector2(m_Player.Rb.linearVelocity.x, m_Player.JumpForce);
    }

    public void Update()
    {
        m_Player.Rb.linearVelocity = new Vector2(m_Player.MovementInput.x * m_Player.Stats.MoveSpeed.Value, m_Player.Rb.linearVelocity.y);

        if (m_Player.Rb.linearVelocity.y <= 0.01f && m_Player.Rb.linearVelocity.y >= -0.01f)
        {
            if (m_Player.MovementInput.x != 0) m_Player.ChangeState(new PlayerWalkState(m_Player));
            else m_Player.ChangeState(new PlayerIdleState(m_Player));
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
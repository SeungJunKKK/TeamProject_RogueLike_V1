using UnityEngine;

public class EnforcerToggleShieldState : IState
{
    private EnforcerController m_Player;
    private float m_Duration = 0.5f; 
    private float m_Timer;
    private bool m_WasDefendingBefore;

    public EnforcerToggleShieldState(EnforcerController player)
    {
        m_Player = player;
    }

    public void Enter()
    {
        m_Timer = 0f;
        m_Player.Rb.linearVelocity = new Vector2(0f, m_Player.Rb.linearVelocity.y);
        m_WasDefendingBefore = m_Player.IsDefending;

        if (m_Player.IsDefending)
        {
            m_Player.Anim.Play("Enforcer_Shield_End");
            m_Player.ToggleShieldStance();
        }
        else
        {
            m_Player.Anim.Play("Enforcer_Shield_Start");
            m_Player.PlayAddressableSFX(m_Player.C_SFXAddress);
            m_Player.ToggleShieldStance(); 
        }

        m_Player.CooldownManager.UseSkill(SkillType.Utility_C);
    }

    public void Update()
    {
        m_Timer += Time.deltaTime;

        if (m_Timer >= m_Duration)
        {
            if (m_Player.MovementInput.x != 0)
            {
                m_Player.ChangeState(new PlayerWalkState(m_Player));
            }
            else
            {
                m_Player.ChangeState(new PlayerIdleState(m_Player));
            }
        }
    }

    public void Exit()
    {
       
    }
}
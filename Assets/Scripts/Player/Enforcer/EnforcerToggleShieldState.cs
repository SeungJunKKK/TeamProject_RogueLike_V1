using UnityEngine;

public class EnforcerToggleShieldState : IState
{
    private EnforcerController m_Player;

    private readonly float m_BaseDuration = 0.5f;
    private readonly float m_SpeedMultiplier = 2f; // 여기 값만 바꾸면 애니메이션/상태 지속시간이 같이 조절됨. 스탯과 연동하려면 이 자리에서 교체.

    private float m_Duration;
    private float m_Timer;
    private bool m_WasDefendingBefore;

    public EnforcerToggleShieldState(EnforcerController player)
    {
        m_Player = player;
    }

    public void Enter()
    {
        m_Timer = 0f;
        m_Duration = m_BaseDuration / m_SpeedMultiplier;

        m_Player.Rb.linearVelocity = new Vector2(0f, m_Player.Rb.linearVelocity.y);
        m_WasDefendingBefore = m_Player.IsDefending;

        m_Player.Anim.speed = m_SpeedMultiplier;

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
        m_Player.Anim.speed = 1f;
    }
}
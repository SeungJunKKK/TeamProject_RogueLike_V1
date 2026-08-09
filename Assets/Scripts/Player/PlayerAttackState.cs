using UnityEngine;

/// <summary>
/// 플레이어 공격 상태를 나타내는 추상 클래스입니다.
/// 여러 캐릭터의 공격 상태를 구현할 때 상속하여 사용할 수 있도록 설계.
/// </summary>
public abstract class PlayerAttackState : IState
{
    protected readonly PlayerController m_Player;
    protected readonly string m_AnimName;
    protected float m_AttackTimer;

    protected readonly SkillType m_SkillType;
    protected bool m_CanCancel = false;

    public PlayerAttackState(PlayerController player, string animName, float duration, SkillType skillType)
    {
        m_Player = player;
        m_AnimName = animName;
        m_AttackTimer = duration;
        m_SkillType = skillType;
    }

    public virtual void Enter()
    {
        m_Player.Anim.speed = m_Player.Stats.AttackSpeed.Value;
        m_Player.Rb.linearVelocity = Vector2.zero;
        m_Player.Anim.Play(m_AnimName);
        m_Player.CooldownManager.UseSkill(m_SkillType);
        m_CanCancel = false;
        //ExecuteShoot();
    }

    public virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && m_CanCancel)
        {
            m_Player.ChangeState(new PlayerDashState(m_Player));
            return;
        }

        m_AttackTimer -= Time.deltaTime;

        if (m_AttackTimer <= 0f)
        {
            m_Player.ChangeState(new PlayerIdleState(m_Player));
        }
    }
    public virtual void Exit()
    {
        m_Player.Anim.speed = 1.0f;
    }
    public virtual void OnActionTriggered()
    {
        ExecuteShoot();
    }
    public void SetCanCancel(bool canCancel)
    {
        m_CanCancel = canCancel;
    }


    protected abstract void ExecuteShoot();
}
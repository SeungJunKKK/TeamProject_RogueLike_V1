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

    public PlayerAttackState(PlayerController player, string animName, float duration)
    {
        m_Player = player;
        m_AnimName = animName;
        m_AttackTimer = duration;
    }

    public virtual void Enter()
    {
        m_Player.Rb.linearVelocity = Vector2.zero;
        m_Player.Anim.Play(m_AnimName);

        ExecuteShoot();
    }

    public virtual void Update()
    {
        m_AttackTimer -= Time.deltaTime;

        if (m_AttackTimer <= 0f)
        {
            m_Player.ChangeState(new PlayerIdleState(m_Player));
        }
    }

    public virtual void Exit()
    {
        // 상태를 나갈 때의 초기화 
    }

    protected abstract void ExecuteShoot();
}
using UnityEngine;

public class PlayerDashState : IState
{
    private readonly PlayerController m_Player;
    private float m_DashTimer;
    private Vector2 m_DashDirection;

    public PlayerDashState(PlayerController player)
    {
        this.m_Player = player;
    }

    public void Enter()
    {
        m_Player.SpawnDustEffect(m_Player.IsFacingRight, EDustType.Dash);
        m_Player.Anim.Play("Dash");

        m_DashTimer = m_Player.DashDuration;
        float dirX = m_Player.MovementInput.x != 0 ? Mathf.Sign(m_Player.MovementInput.x) : (m_Player.IsFacingRight ? 1f : -1f);
        m_DashDirection = new Vector2(dirX, 0f);

        m_Player.IsInvincible = true;
        Debug.Log("시스템: 플레이어 대시 상태 진입. 무적 상태 시작.");
        m_Player.gameObject.layer = LayerMask.NameToLayer("PlayerDodge");
        m_Player.CooldownManager.UseSkill(SkillType.Utility_C);

        m_Player.Rb.linearVelocity = m_DashDirection * m_Player.DashSpeed;
        m_Player.Rb.gravityScale = 0f;
    }

    public void Update()
    {
        m_DashTimer -= Time.deltaTime;

        m_Player.Rb.linearVelocity = m_DashDirection * m_Player.DashSpeed;

        if (m_DashTimer <= 0)
        {
            if (m_Player.MovementInput.x != 0)
                m_Player.ChangeState(new PlayerWalkState(m_Player));
            else
                m_Player.ChangeState(new PlayerIdleState(m_Player));
        }
    }

    public void Exit()
    {
        m_Player.IsInvincible = false;
        m_Player.gameObject.layer = m_Player.OriginalLayer;
        Debug.Log("시스템: 플레이어 대시 상태 종료. 무적 상태 해제.");
        m_Player.Rb.gravityScale = 3f;
        m_Player.Rb.linearVelocity = Vector2.zero;
    }
}
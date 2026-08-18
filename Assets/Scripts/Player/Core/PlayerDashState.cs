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
        m_Player.PlayAddressableSFX(m_Player.Dash_SFXAddress);
        m_DashTimer = m_Player.DashDuration;
        float dirX = m_Player.MovementInput.x != 0 ? Mathf.Sign(m_Player.MovementInput.x) : (m_Player.IsFacingRight ? 1f : -1f);
        m_DashDirection = new Vector2(dirX, 0f);

        m_Player.IsInvincible = true;
        m_Player.gameObject.layer = LayerMask.NameToLayer("PlayerDodge");
        m_Player.CooldownManager.UseSkill(SkillType.Utility_C);

        //m_Player.Rb.linearVelocity = m_DashDirection * m_Player.DashSpeed;
        //m_Player.Rb.gravityScale = 0f;

        m_Player.Rb.linearVelocity = new Vector2(m_DashDirection.x * m_Player.DashSpeed, m_Player.Rb.linearVelocity.y);
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

        m_DashTimer -= Time.deltaTime;

        int wallLayer = LayerMask.GetMask("Default", "Ground", "Wall");

        Vector2 rayStart = (Vector2)m_Player.transform.position + new Vector2(0f, 0.25f);

        bool isWallAhead = Physics2D.Raycast(rayStart, m_DashDirection, 0.3f, wallLayer);

        Debug.DrawRay(rayStart, m_DashDirection * 0.3f, Color.red);

        if (isWallAhead)
        {
            m_Player.Rb.linearVelocity = new Vector2(0f, m_Player.Rb.linearVelocity.y);
        }
        else
        {
            m_Player.Rb.linearVelocity = new Vector2(m_DashDirection.x * m_Player.DashSpeed, m_Player.Rb.linearVelocity.y);
        }

        if (m_DashTimer <= 0)
        {
            if (Mathf.Abs(m_Player.Rb.linearVelocity.y) > 0.1f)
            {
                m_Player.ChangeState(new PlayerFallState(m_Player));
            }
            else if (m_Player.MovementInput.x != 0)
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
        m_Player.IsInvincible = false;
        m_Player.gameObject.layer = m_Player.OriginalLayer;
        m_Player.Rb.linearVelocity = new Vector2(0f, m_Player.Rb.linearVelocity.y);
    }
}
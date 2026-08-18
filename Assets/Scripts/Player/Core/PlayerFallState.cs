using UnityEngine;

public class PlayerFallState : IState
{
    private PlayerController m_Player;

    public PlayerFallState(PlayerController player) { this.m_Player = player; }

    public void Enter()
    {
        m_Player.Anim.Play("Jump");
    }

    public void Update()
    {
        m_Player.Rb.linearVelocity = new Vector2(m_Player.MovementInput.x * m_Player.Stats.MoveSpeed.Value, m_Player.Rb.linearVelocity.y);

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

        //바닥 체크 
        int groundLayer = LayerMask.GetMask("Default", "Ground", "OneWayGround", "Wall");
        //bool isGrounded = Physics2D.Raycast(m_Player.FeetPos.position, Vector2.down, 0.15f, groundLayer);

        Vector2 boxSize = new Vector2(0.3f, 0.05f);
        RaycastHit2D groundHit = Physics2D.BoxCast(m_Player.FeetPos.position, boxSize, 0f, Vector2.down, 0.1f, groundLayer);
        bool isGrounded = groundHit.collider != null;

        
        if (isGrounded && m_Player.Rb.linearVelocity.y <= 0.05f)
        {
            if (m_Player.MovementInput.x != 0)
            {
                m_Player.ChangeState(new PlayerWalkState(m_Player));
            }
            else
            {
                m_Player.ChangeState(new PlayerIdleState(m_Player));
            }
            return;
        }

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
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.C)) && m_Player.CooldownManager.IsSkillReady(SkillType.Utility_C))
        {
            m_Player.ChangeState(new PlayerDashState(m_Player));
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
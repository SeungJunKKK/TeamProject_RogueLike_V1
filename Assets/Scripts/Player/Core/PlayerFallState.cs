using UnityEngine;

public class PlayerFallState : IState
{
    private PlayerController m_Player;

    private int m_PlayerLayer;
    private int m_OneWayLayer;
    private bool m_IsPassingThrough;

    public PlayerFallState(PlayerController player) { this.m_Player = player; }

    public void Enter()
    {
        m_Player.Anim.Play("Jump");

        m_PlayerLayer = m_Player.gameObject.layer;
        m_OneWayLayer = LayerMask.NameToLayer("OneWayGround");

        Collider2D playerCol = m_Player.GetComponent<Collider2D>();
        if (playerCol != null && m_OneWayLayer != -1)
        {
            int layerMask = 1 << m_OneWayLayer;
            Collider2D hit = Physics2D.OverlapBox(playerCol.bounds.center, playerCol.bounds.size, 0f, layerMask);

            if (hit != null)
            {
                m_IsPassingThrough = true;
                Physics2D.IgnoreLayerCollision(m_PlayerLayer, m_OneWayLayer, true);
            }
        }

    }

    public void Update()
    {
        if (m_IsPassingThrough)
        {
            Collider2D playerCol = m_Player.GetComponent<Collider2D>();
            int layerMask = 1 << m_OneWayLayer;
            Collider2D hit = Physics2D.OverlapBox(playerCol.bounds.center, playerCol.bounds.size, 0f, layerMask);

            if (hit == null) // 겹친 곳 없이 완전히 빠져나왔다면
            {
                // 통과 모드 OFF, 다시 발판을 밟을 수 있게 충돌 복구!
                m_IsPassingThrough = false;
                Physics2D.IgnoreLayerCollision(m_PlayerLayer, m_OneWayLayer, false);
            }
        }

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

        int groundLayer = LayerMask.GetMask("Default", "Ground", "Wall");
        if (!m_IsPassingThrough)
        {
            groundLayer |= (1 << m_OneWayLayer);
        }


        Vector2 boxSize = new Vector2(0.4f, 0.1f);
        RaycastHit2D groundHit = Physics2D.BoxCast(m_Player.FeetPos.position, boxSize, 0f, Vector2.down, 0.2f, groundLayer);
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

    public void Exit()
    {
        if (m_IsPassingThrough && m_OneWayLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(m_PlayerLayer, m_OneWayLayer, false);
            m_IsPassingThrough = false;
        }
    }
}
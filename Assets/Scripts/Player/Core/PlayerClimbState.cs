using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerClimbState : IState
{
    private PlayerController m_Player;
    private Collider2D m_LadderCollider;
    private float m_OriginalGravity;


    public PlayerClimbState(PlayerController player, Collider2D ladderCollider)
    {
        m_Player = player;
        m_LadderCollider = ladderCollider;
    }

    public void Enter()
    {
        m_Player.Rb.bodyType = RigidbodyType2D.Kinematic;
        m_Player.Rb.linearVelocity = Vector2.zero;

        m_Player.Anim.Play("Climb"); 
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_Player.ChangeState(new PlayerJumpState(m_Player));
            return;
        }

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(horizontalInput) > 0.5f)
        {
            m_Player.ChangeState(new PlayerFallState(m_Player));
            return;
        }

        if (!m_Player.IsTouchingLadder())
        {
            m_Player.ChangeState(new PlayerFallState(m_Player));
            return;
        }

        // ================= 기존 이동 로직 =================
        float verticalInput = Input.GetAxisRaw("Vertical");
        float currentMoveSpeed = m_Player.Stats.MoveSpeed.Value > 0f ? m_Player.Stats.MoveSpeed.Value : 4f;
        float climbSpeed = currentMoveSpeed * 1.0f;

        m_Player.Rb.linearVelocity = new Vector2(0f, verticalInput * climbSpeed);

        if (verticalInput != 0f) m_Player.Anim.speed = 1f;
        else m_Player.Anim.speed = 0f;
    }

    public void Exit()
    {
        m_Player.Rb.bodyType = RigidbodyType2D.Dynamic;
        m_Player.Anim.speed = 1f;
    }
}
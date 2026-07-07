using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private IState m_CurrentState;

    public Rigidbody2D Rb { get; private set; }
    public Animator Anim { get; private set; }
    public SpriteRenderer SpriteRendererComponent { get; private set; }

    [Header("Player Stats")]
    public float MoveSpeed = 5f;
    public float JumpForce = 12f;
    public float DashSpeed = 15f;
    public float DashDuration = 0.2f;

    [Header("Effects")]
    public GameObject DustPrefab;
    public Transform FeetPos;

    public Vector2 MovementInput { get; private set; }
    public bool IsFacingRight { get; private set; } = true;

    private void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        Anim = GetComponentInChildren<Animator>();
        SpriteRendererComponent = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        ChangeState(new PlayerIdleState(this));
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        MovementInput = new Vector2(horizontalInput, verticalInput).normalized;

        if (MovementInput.x > 0)
        {
            IsFacingRight = true;
            SpriteRendererComponent.flipX = false; 
        }
        else if (MovementInput.x < 0)
        {
            IsFacingRight = false;
            SpriteRendererComponent.flipX = true;  
        }

        if (m_CurrentState != null) m_CurrentState.Update();
    }

    public void ChangeState(IState newState)
    {
        if (m_CurrentState != null) m_CurrentState.Exit();
        m_CurrentState = newState;
        m_CurrentState.Enter();
    }

    public void SpawnDustEffect(bool isFacingRight)
    {
        if (DustPrefab != null && FeetPos != null)
        {
            GameObject dust = Instantiate(DustPrefab, FeetPos.position, Quaternion.identity);
            Animator dustAnim = dust.GetComponent<Animator>();
            if (dustAnim != null)
            {
                dustAnim.Play(isFacingRight ? "Dust_Right" : "Dust_Left");
            }
        }
    }
    public virtual IState GetPrimaryAttackState() { return null; }
    public virtual IState GetSecondaryAttackState() { return null; }
}
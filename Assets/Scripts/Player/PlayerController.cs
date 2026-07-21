using System.Collections;
using Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private IState m_CurrentState;

    public Rigidbody2D Rb { get; private set; }
    public Animator Anim { get; private set; }
    public bool IsInvincible { get; set; } = false; // 무적 상태 스위치
    public int OriginalLayer { get; private set; }  // 원래 레이어 기억용
    public virtual IState GetPrimaryAttackState() { return null; }
    public virtual IState GetSecondaryAttackState() { return null; }
    public virtual IState GetUtilitySkillState() { return null; }
    public virtual IState GetUltimateSkillState() { return null; }

    public SpriteRenderer SpriteRendererComponent { get; private set; }

    [Header("Player Stats")]
    public float MoveSpeed = 5f;
    public float JumpForce = 12f;
    [Header("Effects")]
    public GameObject DustPrefab;
    public Transform FeetPos;
    [Header("Dash Settings")]
    public float DashSpeed = 15f;
    public float DashDuration = 0.2f;
    [Header("Hit Feedback")]
    public CinemachineImpulseSource ImpulseSource;
    [Header("Skill System")]
    public SkillCooldownManager CooldownManager;
    [Header("Skill Prefabs")]
    public GameObject DoubleTapProjectilePrefab;
    public GameObject FullMetalJacketPrefab;
    public GameObject SuppressiveFireVFXPrefab; 
    public GameObject SuppressiveBarrageVFXPrefab;


    [Header("Muzzle Position")]
    public Transform MuzzlePos;


    public Vector2 MovementInput { get; private set; }

    public bool IsFacingRight { get; private set; } = true;

    private void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        Anim = GetComponentInChildren<Animator>();
        SpriteRendererComponent = GetComponentInChildren<SpriteRenderer>();
        CooldownManager = GetComponent<SkillCooldownManager>();
        OriginalLayer = gameObject.layer;
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
            //SpriteRendererComponent.flipX = false;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (MovementInput.x < 0)
        {
            IsFacingRight = false;
            //SpriteRendererComponent.flipX = true;
            transform.rotation = Quaternion.Euler(0, 180, 0);
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

            Destroy(dust, 0.5f);
        }
    }
    public void TriggerHitFeedback(Vector2 direction, float shakeForce, float hitStopDuration)
    {
        if (ImpulseSource != null)
        {
            ImpulseSource.GenerateImpulse(new Vector3(direction.x * shakeForce, 0f, 0f));
        }
        StartCoroutine(HitStopRoutine(hitStopDuration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    public void OnSkillActionTrigger()
    {
        if (m_CurrentState is PlayerAttackState attackState)
        {
            attackState.OnActionTriggered();
        }
    }

}
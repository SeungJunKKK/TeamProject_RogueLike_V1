using Cinemachine;
using System;
using System.Collections;
using UnityEngine;

public enum EDustType
{
    Dash,
    Jump,
    Recoil
}
public class PlayerController : MonoBehaviour
{
    private IState m_CurrentState;

    public PlayerStats Stats { get; private set; }
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
    public float JumpForce = 5f;
    [Header("Effects (Addressable)")]
    public string DustAddress = "PlayerDustEffect";
    public Transform FeetPos;
    [Header("Dash Settings")]
    public float DashSpeed = 5f;
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
        Stats = GetComponent<PlayerStats>();
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

        //===============================아이템 테스트===============================
        if (Input.GetKeyDown(KeyCode.I))
        {
            EventBus.Publish(new ItemPickedUpEvent
            {
                ItemName = "군인의 주사기",
                TargetStat = EStatType.AttackSpeed,
                Modifier = new StatModifier(0.15f, StatModType.PercentAdd, "Syringe")
            });
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            EventBus.Publish(new ItemPickedUpEvent
            {
                ItemName = "안경 메이커의 안경",
                TargetStat = EStatType.CritChance,
                Modifier = new StatModifier(0.10f, StatModType.Flat, "Glasses")
            });
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            Stats.AddExp(50f);
        }
        //===============================아이템 테스트===============================



    }

    public void ChangeState(IState newState)
    {
        if (m_CurrentState != null) m_CurrentState.Exit();
        m_CurrentState = newState;
        m_CurrentState.Enter();
    }

    public void SpawnDustEffect(bool isFacingRight, EDustType dustType)
    {
        if (FeetPos == null)
        {
            return;
        }

        AddressableManager.Instance.LoadAssetAsync<GameObject>(DustAddress, (prefab) =>
        {
            if (prefab != null)
            {
                Vector3 spawnPos = FeetPos.position;
                string animName = "";

                switch (dustType)
                {
                    case EDustType.Dash:
                        animName = isFacingRight ? "Dash_Right_Dust" : "Dash_Left_Dust";
                        break;
                    case EDustType.Jump:
                        animName = isFacingRight ? "Jump_Right_Dust" : "Jump_Left_Dust";
                        break;
                    case EDustType.Recoil:
                        animName = isFacingRight ? "Dash_Left_Dust" : "Dash_Right_Dust";
                        float offsetX = isFacingRight ? -0.5f : 0.5f;
                        spawnPos = new Vector3(spawnPos.x + offsetX, spawnPos.y, spawnPos.z);
                        break;
                    default:
                        throw new NotImplementedException($"unhandled: {dustType}");
                }

                GameObject dust = Instantiate(prefab, spawnPos, Quaternion.identity);
                Animator dustAnim = dust.GetComponent<Animator>();

                if (dustAnim != null)
                {
                    dustAnim.Play(animName);
                }

                Destroy(dust, 0.5f);
            }
        });
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
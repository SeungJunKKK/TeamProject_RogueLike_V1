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
    private bool m_IsHitStopping = false;

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
    [Header("Skill Sounds (Addressable)")]
    public string Z_SFXAddress = "";
    public string X_SFXAddress = "";
    public string V_SFXAddress = "";
    public string VStrengthened_SFXAddress = "";
    [Header("Ladder Settings")]
    public LayerMask LadderLayer; 
    public float LadderCheckDistance = 0.5f; 
    [Header("Muzzle Position")]
    public Transform MuzzlePos;
    [Header("Melee Hitbox")]
    public Collider2D MeleeCollider;



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
        if (Input.GetKeyDown(KeyCode.UpArrow)) // 위 방향키 누를 때마다 확인
        {
            Collider2D hit = CheckLadderUp();
            Debug.Log($"<color=yellow>[사다리 탐지기]</color> 위쪽 사다리 감지 결과: {(hit != null ? hit.name : "찾을 수 없음 (Null)")}");
        }


    }

    public void ChangeState(IState newState)
    {
        if (m_CurrentState != null) m_CurrentState.Exit();
        m_CurrentState = newState;
        m_CurrentState.Enter();
    }

    /// <summary>
    /// 어드레서블 주소로 오디오 클립을 비동기 로드하여 사운드 매니저를 통해 재생합니다.
    /// </summary>
    public void PlayAddressableSFX(string sfxAddress)
    {
        if (string.IsNullOrEmpty(sfxAddress)) return;

        AddressableManager.Instance.LoadAssetAsync<AudioClip>(sfxAddress, (clip) =>
        {
            if (clip != null)
            {
                SoundManager.Instance.PlaySFX(clip);
            }
            else
            {
                Debug.LogError($"[어드레서블 에러] '{sfxAddress}' 주소로 효과음을 찾을 수 없습니다!");
            }
        });
    }

    public void SpawnDustEffect(bool isFacingRight, EDustType dustType)
    {
        if (FeetPos == null)
        {
            Debug.LogWarning(" FeetPos가 할당되지 않았습니다 ,인스펙터를 확인하세요.");
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
            else 
            {
                Debug.LogError($"[어드레서블 에러] '{DustAddress}' 주소로 프리팹을 찾을 수 없습니다! " +
                    $"Addressables Groups 창의 Key와 이름이 똑같은지 확인하세요.");
            }

        });
    }

    /// <summary>
    /// 플레이어의 몸이 현재 사다리 영역(LadderLayer)과 겹쳐있는지 검사합니다.
    /// </summary>
    public bool IsTouchingLadder()
    {
        // 플레이어 몸통 중심점
        Vector2 centerPos = (Vector2)transform.position + new Vector2(0f, 0.2f);
        Collider2D col = Physics2D.OverlapCircle(centerPos, 0.2f, LadderLayer);
        return col != null;
    }


    /// <summary>
    /// 플레이어 몸 중심에서 위로 레이저를 쏴서 사다리를 감지합니다. (사다리 앞에서 위 방향키 누를 때 사용)
    /// </summary>
    public Collider2D CheckLadderUp()
    {
       
        Debug.DrawRay(transform.position, Vector2.up * LadderCheckDistance, Color.red, 2f);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, LadderCheckDistance, LadderLayer);
        return hit.collider;
    }

    /// <summary>
    /// 플레이어 발밑에서 아래로 레이저를 쏴서 사다리를 감지합니다. (바닥이나 발판 위에서 아래 방향키 누를 때 사용)
    /// </summary>
    public Collider2D CheckLadderDown()
    {
        if (FeetPos == null) return null;

        RaycastHit2D hit = Physics2D.Raycast(FeetPos.position, Vector2.down, LadderCheckDistance, LadderLayer);
        return hit.collider;
    }
    /// <summary>
    /// Scene 뷰에서 플레이어 몸 중심과 발밑에서 레이저를 시각적으로 표시합니다. (디버그용)
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector2 centerPos = (Vector2)transform.position + new Vector2(-0.04f, 0.2f);
        Gizmos.DrawRay(centerPos, Vector2.up * LadderCheckDistance);

        if (FeetPos != null)
        {
            Gizmos.DrawRay(FeetPos.position, Vector2.down * LadderCheckDistance);
        }
    }

    public void TriggerHitFeedback(Vector2 direction, float shakeForce, float hitStopDuration)
    {
        if (ImpulseSource != null)
        {
            ImpulseSource.GenerateImpulse(new Vector3(direction.x * shakeForce, 0f, 0f));
        }

        if (!m_IsHitStopping && hitStopDuration > 0f)
        {
            StartCoroutine(HitStopRoutine(hitStopDuration));
        }
    }

    /// <summary>
    /// 싱글 플레이 기반으로 플레이어가 공격을 맞았을 때 잠시 시간을 멈추는 효과를 발생시킵니다. (Hit Stop)
    /// 멀티 플레이에서는 사용하지 않을 계획입니다. 
    /// </summary>
    /// <param name="duration">멈춤 지속 시간</param>
    public void TriggerHitStop(float duration = 0.05f)
    {
        if (!m_IsHitStopping && duration > 0f)
        {
            StartCoroutine(HitStopRoutine(duration));
        }
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        m_IsHitStopping = true;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        m_IsHitStopping = false;
    }

    public void OnSkillActionTrigger()
    {
        if (m_CurrentState is PlayerAttackState attackState)
        {
            attackState.OnActionTriggered();
        }
    }

    /// <summary>
    /// 애니메이션 이벤트에서 호출하여, 공격 후딜레이 캔슬(대시 등)을 허용합니다.
    /// </summary>
    public void EnableActionCancel()
    {
        if (m_CurrentState is PlayerAttackState attackState)
        {
            attackState.SetCanCancel(true);
        }
    }
    public void EnableHitbox()
    {
        Debug.Log("<color=cyan>[PlayerController] EnableHitbox 호출 - 공격 판정 ON</color>");

        if (MeleeCollider != null)
        {
            MeleeCollider.enabled = true;
        }
        else
        {
            Debug.LogWarning("[PlayerController] MeleeCollider가 인스펙터에 할당되지 않았습니다");
        }
    }

    public void DisableHitbox()
    {
        Debug.Log("<color=cyan>[PlayerController] DisableHitbox 호출 - 공격 판정 OFF</color>");

        if (MeleeCollider != null)
        {
            MeleeCollider.enabled = false;
        }
    }


}
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Animator))]
public class BossSpreadingEffect : MonoBehaviour
{
    [SerializeField] private float m_LifeTime = 0.6f;
    private float m_Damage;
    private int m_Direction;
    private GameObject m_Attacker;
    private bool m_IsInitialized = false;

    private Animator m_Animator;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }
    public void Setup(float damage, int direction, GameObject attacker, bool isBigPillar)
    {
        m_Damage = damage;
        m_Direction = direction;
        m_Attacker = attacker;

        // 방향 전환
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;

        if (isBigPillar)
        {
            m_Animator.Play("BigWave");
        }
        else
        {
            m_Animator.Play("SmallWave");
        }

        m_IsInitialized = true;
        Destroy(gameObject, m_LifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!m_IsInitialized) return;

        IDamageable target = collision.GetComponent<IDamageable>() ?? collision.GetComponentInParent<IDamageable>();
        if (target != null)
        {
            DamageInfo info = new DamageInfo
            {
                Amount = m_Damage,
                HitPoint = collision.ClosestPoint(transform.position),
                HitDirection = new Vector2(m_Direction, 0.5f),
                KnockbackForce = 12f,
                Attacker = m_Attacker,
                IsCrit = false,
                CanProc = true
            };
            target.TakeDamage(info);
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
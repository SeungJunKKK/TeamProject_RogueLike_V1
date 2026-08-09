using UnityEngine;

public class TestDummyHealth : MonoBehaviour, IDamageable
{
    public float MaxHealth = 100f;
    private float m_CurrentHealth;
    private Rigidbody2D m_Rb;

    private void Awake()
    {
        m_Rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        m_CurrentHealth = MaxHealth;
    }

    public void TakeDamage(DamageInfo info)
    {
        m_CurrentHealth -= info.Amount;

        if (info.IsCrit)
        {
            Debug.Log($"<color=yellow>[크리티컬!]</color> 입은 데미지: {info.Amount} / 남은 체력: {m_CurrentHealth}");
        }
        else
        {
            Debug.Log($"<color=red>맞았다!</color> 입은 데미지: {info.Amount} / 남은 체력: {m_CurrentHealth}");
        }

        EventBus.Publish(new MonsterDamagedEvent
        {
            Amount = info.Amount,
            HitPoint = info.HitPoint,
            IsCrit = info.IsCrit
        });

        if (m_Rb != null)
        {
            m_Rb.AddForce(info.HitDirection * info.KnockbackForce, ForceMode2D.Impulse);
        }

        if (m_CurrentHealth <= 0)
        {
            Debug.Log($"{gameObject.name} 파괴됨!");
            GameManager.Instance.AddGold(50);
            Destroy(gameObject);
        }
    }
}
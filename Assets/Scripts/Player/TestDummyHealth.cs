using UnityEngine;

public class TestDummyHealth : MonoBehaviour
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

    public void TakeDamage(float damage, Vector2 knockbackDir, float knockbackForce)
    {
        m_CurrentHealth -= damage;

        Debug.Log($"[적중] {gameObject.name}이 {damage}의 피해를 입었습니다. (남은 체력: {m_CurrentHealth})");

        if (m_Rb != null)
        {
            m_Rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }

        if (m_CurrentHealth <= 0)
        {
            Debug.Log($"{gameObject.name} 파괴됨!");
            Destroy(gameObject); 
        }
    }
}
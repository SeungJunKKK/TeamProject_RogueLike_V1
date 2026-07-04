using UnityEngine;

public class DummyHealth : MonoBehaviour
{
    public float MaxHealth = 100f;
    private float m_CurrentHealth;

    private void Start()
    {
        m_CurrentHealth = MaxHealth;
    }

    public void TakeDamage(float damage)
    {
        m_CurrentHealth -= damage;

        Debug.Log($"[적중] {gameObject.name}이 {damage}의 피해를 입었습니다. (남은 체력: {m_CurrentHealth})");

        if (m_CurrentHealth <= 0)
        {
            Debug.Log($"{gameObject.name} 파괴됨!");
            Destroy(gameObject); 
        }
    }
}
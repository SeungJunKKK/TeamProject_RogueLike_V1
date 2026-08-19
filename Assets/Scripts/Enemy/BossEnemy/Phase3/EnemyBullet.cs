using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float m_Speed = 8f;
    [SerializeField] private float m_Damage = 10f;

    private Vector2 m_Direction;

    public void Setup(Vector2 direction, float damageMultiplier)
    {
        m_Direction = direction.normalized;
        m_Damage *= damageMultiplier;

        // 발사 방향에 맞춰 총알 스프라이트 회전
        float angle = Mathf.Atan2(m_Direction.y, m_Direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Destroy(gameObject, 6f); // 6초 뒤 자동 파괴
    }

    private void Update()
    {
        transform.position += (Vector3)(m_Direction * m_Speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>() ?? collision.GetComponentInParent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(m_Damage);
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground")) // 지면에 닿으면 소멸
        {
            Destroy(gameObject);
        }
    }
}
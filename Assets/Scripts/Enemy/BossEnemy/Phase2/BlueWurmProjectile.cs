using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BlueWurmProjectile : MonoBehaviour
{
    private float m_Damage;
    private Vector2 m_Direction;
    private float m_Speed;
    private bool m_IsInitialized = false;

    public void Setup(float damage, Vector2 direction, float speed, float lifetime)
    {
        m_Damage = damage;
        m_Direction = direction.normalized;
        m_Speed = speed;
        m_IsInitialized = true;

        // 회전값 조정 (날아가는 방향을 바라보도록)
        float angle = Mathf.Atan2(m_Direction.y, m_Direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!m_IsInitialized) return;
        transform.position += (Vector3)(m_Direction * m_Speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!m_IsInitialized) return;

        // 플레이어 피격 처리
        PlayerController player = collision.GetComponent<PlayerController>() ?? collision.GetComponentInParent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(m_Damage);
            Destroy(gameObject);
            return;
        }

        // 지형(Ground)에 부딪히면 소멸
        if (((1 << collision.gameObject.layer) & LayerMask.GetMask("Ground")) != 0)
        {
            Destroy(gameObject);
        }
    }
}
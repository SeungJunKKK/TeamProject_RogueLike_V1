using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class BlueWurmProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float m_Damage = 10f;
    [SerializeField] private float m_Speed = 12f;
    [SerializeField] private float m_Lifetime = 3f;

    [Header("Collision Settings")]
    [SerializeField] private LayerMask m_PlayerLayer;

    private Vector2 m_Direction;
    private Collider2D m_Collider;
    private bool m_IsInitialized;

    private readonly Collider2D[] m_OverlapResults = new Collider2D[8];

    public void Setup(
        float damage,
        Vector2 direction,
        float speed,
        float lifetime)
    {
        m_Damage = damage;
        m_Direction = direction.normalized;
        m_Speed = speed;
        m_Lifetime = lifetime;

        m_IsInitialized = true;

        float angle =
            Mathf.Atan2(m_Direction.y, m_Direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, m_Lifetime);
    }

    private void Awake()
    {
        m_Collider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (!m_IsInitialized)
        {
            return;
        }

        MoveProjectile();
        CheckPlayerOverlap();
    }

    private void MoveProjectile()
    {
        transform.position +=
            (Vector3)(m_Direction * m_Speed * Time.deltaTime);
    }

    private void CheckPlayerOverlap()
    {
        Physics2D.SyncTransforms();

        ContactFilter2D filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = m_PlayerLayer,
            useTriggers = true
        };

        int hitCount = Physics2D.OverlapCollider(
            m_Collider,
            filter,
            m_OverlapResults
        );

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hitCollider = m_OverlapResults[i];

            if (hitCollider == null)
            {
                continue;
            }

            PlayerController player = hitCollider.GetComponent<PlayerController>();

            if (player == null)
            {
                player = hitCollider.GetComponentInParent<PlayerController>();
            }

            if (player == null)
            {
                continue;
            }

            player.TakeDamage(m_Damage);

            Destroy(gameObject);
            return;
        }
    }
}
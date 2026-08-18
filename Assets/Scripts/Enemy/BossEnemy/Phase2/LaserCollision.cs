using UnityEngine;

public class LaserCollision : MonoBehaviour
{
    [SerializeField] private float m_BeamDamage = 15f;
    [SerializeField] private float m_DamageInterval = 0.08f;

    private float m_DamageTimer = 0f;
    private bool m_IsPlayerInside = false;
    private PlayerController m_TargetPlayer;

    private void OnEnable()
    {
        m_IsPlayerInside = false;
        m_DamageTimer = 0f;
        m_TargetPlayer = null;
    }

    private void Update()
    {
        if (m_IsPlayerInside && m_TargetPlayer != null)
        {
            m_DamageTimer += Time.deltaTime;
            if (m_DamageTimer >= m_DamageInterval)
            {
                m_DamageTimer = 0f;
                m_TargetPlayer.TakeDamage(m_BeamDamage);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>() ?? collision.GetComponentInParent<PlayerController>();
        if (player != null)
        {
            m_IsPlayerInside = true;
            m_TargetPlayer = player;
            m_DamageTimer = m_DamageInterval; // 닿자마자 바로 틱이 들어가도록 설정
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>() ?? collision.GetComponentInParent<PlayerController>();
        if (player != null && player == m_TargetPlayer)
        {
            m_IsPlayerInside = false;
            m_TargetPlayer = null;
            m_DamageTimer = 0f;
        }
    }
}
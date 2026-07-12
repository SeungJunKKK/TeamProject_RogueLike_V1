using UnityEngine;
using UnityEngine.InputSystem;

public class DummySelfHit : MonoBehaviour
{
    [SerializeField]
    private float m_KnockbackForce = 3f;

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            GetComponent<IDamageable>()?.TakeDamage(new DamageInfo
            {
                Amount = Random.Range(10, 100),
                IsCrit = Random.value < 0.3f,
                HitPoint = (Vector2)transform.position + Random.insideUnitCircle * 0.5f,
                HitDirection = Random.insideUnitCircle.normalized,
                KnockbackForce = m_KnockbackForce,
                Attacker = gameObject
            });
        }
    }
}

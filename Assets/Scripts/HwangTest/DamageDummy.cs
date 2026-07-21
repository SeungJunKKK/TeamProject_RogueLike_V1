using System.Collections;
using UnityEngine;

public class DamageDummy : MonoBehaviour, IDamageable
{
    private Rigidbody2D m_RigidBody;
    private Vector2 m_OriginPos;
    private Coroutine m_ResetCoroutine;

    private void Awake()
    {
        m_RigidBody = GetComponent<Rigidbody2D>();
        m_OriginPos = transform.position;
    }

    public void TakeDamage(DamageInfo info)
    {
        Debug.Log($"[Dummy] Amount:{info.Amount} Crit:{info.IsCrit} Knockback:{info.KnockbackForce} " +
              $"Dir:{info.HitDirection} Attacker:{info.Attacker?.name} CanProc:{info.CanProc}");
        EventBus.Publish(new MonsterDamagedEvent { Amount = info.Amount, HitPoint = info.HitPoint, IsCrit = info.IsCrit });
        m_RigidBody.AddForce(info.HitDirection * info.KnockbackForce, ForceMode2D.Impulse);

        if (m_ResetCoroutine != null)
            StopCoroutine(m_ResetCoroutine);
        m_ResetCoroutine = StartCoroutine(ResetRoutine());
    }

    private IEnumerator ResetRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        transform.position = m_OriginPos;
        m_RigidBody.linearVelocity = Vector2.zero;
        m_ResetCoroutine = null;
    }
}

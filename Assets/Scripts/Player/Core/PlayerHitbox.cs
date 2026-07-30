using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public PlayerController Player;
    public float DamageMultiplier = 1.0f; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
       
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
              
                bool isCrit = Player.Stats.RollCriticalHit();
                float baseDamage = Player.Stats.Damage.Value * DamageMultiplier;
                float finalDamage = isCrit ? baseDamage * Player.Stats.CritDamage.Value : baseDamage;
               
                Vector2 hitDirection = Player.IsFacingRight ? Vector2.right : Vector2.left;
                DamageInfo info = new DamageInfo
                {
                    Amount = 2f,
                    HitPoint = collision.ClosestPoint(transform.position),
                    HitDirection = hitDirection,
                    KnockbackForce = 10f,
                    Attacker = Player.gameObject,
                    IsCrit = isCrit,
                    CanProc = true
                };

                damageable.TakeDamage(info);
                Player.TriggerHitFeedback(hitDirection, 0.3f, 0.05f);
            }
        }
    }
}
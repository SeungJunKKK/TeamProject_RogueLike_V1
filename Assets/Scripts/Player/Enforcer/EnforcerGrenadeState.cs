using UnityEngine;

public class EnforcerGrenadeState : IState
{
    private EnforcerController m_Player;
    private float m_Duration;
    private float m_Timer;
    private bool m_HasThrown = false;

    public EnforcerGrenadeState(EnforcerController player, float duration)
    {
        m_Player = player;
        m_Duration = duration;
    }

    public void Enter()
    {
        m_Timer = 0f;
        m_HasThrown = false;
        m_Player.Rb.linearVelocity = new Vector2(0f, m_Player.Rb.linearVelocity.y);

        // 공격 속도 연동 (스탯에 비례해서 모션이 빨라짐)
        float attackSpeed = Mathf.Max(0.1f, m_Player.Stats.AttackSpeed.Value);
        m_Player.Anim.speed = attackSpeed;
        m_Player.Anim.Play("Grenade_Explosion");

        m_Player.CooldownManager.UseSkill(SkillType.Ultimate_V);
    }

    public void Update()
    {
        m_Timer += Time.deltaTime;

        if (!m_HasThrown && m_Timer >= (m_Duration * 0.4f))
        {
            m_HasThrown = true;
            // 투척 동작 효과음 재생
            m_Player.PlayAddressableSFX(m_Player.V_SFXAddress);

            ThrowGrenade();
        }

        // 상태 종료 조건
        if (m_Timer >= m_Duration)
        {
            if (m_Player.MovementInput.x != 0)
            {
                m_Player.ChangeState(new PlayerWalkState(m_Player));
            }
            else
            {
                m_Player.ChangeState(new PlayerIdleState(m_Player));
            }
        }
    }

    private void ThrowGrenade()
    {

      

        Vector2 spawnPos = m_Player.MuzzlePos != null
            ? (Vector2)m_Player.MuzzlePos.position
            : (Vector2)m_Player.transform.position + new Vector2(m_Player.IsFacingRight ? 0.5f : -0.5f, 0.2f);

        Vector2 throwDir = m_Player.IsFacingRight ? Vector2.right : Vector2.left;

        GameObject grenadePrefab = Resources.Load<GameObject>("EnforcerGrenadeProjectile");
        if (grenadePrefab != null)
        {
            GameObject grenadeObj = PoolManager.Instance.Get(grenadePrefab, spawnPos, Quaternion.identity);
            EnforcerGrenadeProjectile projectile = grenadeObj.GetComponent<EnforcerGrenadeProjectile>();

            if (projectile != null)
            {
                float damage = m_Player.Stats.Damage.Value * 2.5f;
                bool isStrengthened = false;
                projectile.Initialize(throwDir, damage, isStrengthened);
            }
        }
        else
        {
            Debug.LogError("<color=red>[에러] Resources 폴더에 'EnforcerGrenadeProjectile' 프리팹이 없습니다!</color>");
        }
    }

    public void Exit()
    {
        m_Player.Anim.speed = 1f;
    }
}
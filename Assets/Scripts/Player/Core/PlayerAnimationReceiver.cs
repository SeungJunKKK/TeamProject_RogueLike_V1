using UnityEngine;

public class PlayerAnimationReceiver : MonoBehaviour
{
    private PlayerController m_Player;

    private void Awake()
    {
        m_Player = GetComponentInParent<PlayerController>();
    }

    public void EnableActionCancel()
    {
        if (m_Player != null)
        {
            m_Player.EnableActionCancel();
        }
    }

    public void OnSkillActionTrigger()
    {
        if (m_Player != null)
        {
            m_Player.OnSkillActionTrigger();
        }
    }
    public void EnableHitbox()
    {
        if (m_Player != null) m_Player.EnableHitbox();
    }

    public void DisableHitbox()
    {
        if (m_Player != null) m_Player.DisableHitbox();
    }
}
using UnityEngine;

public class PlayerAnimationRelay : MonoBehaviour
{
    private PlayerController m_Player;

    private void Awake()
    {
        m_Player = GetComponentInParent<PlayerController>();
    }

    public void OnSkillActionTrigger()
    {
        if (m_Player != null)
        {
            m_Player.OnSkillActionTrigger();
        }
    }
}
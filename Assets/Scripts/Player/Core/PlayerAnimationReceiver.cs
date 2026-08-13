using UnityEngine;

public class PlayerAnimationReceiver : MonoBehaviour
{
    private PlayerController m_Player;
    private PlayerInventory m_Inventory;

    private void Awake()
    {
        m_Player = GetComponentInParent<PlayerController>();
        m_Inventory = GetComponentInParent<PlayerInventory>();
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
    public void FireBasicAttackEvent()
    {
        if (m_Inventory != null)
        {
            m_Inventory.OnBasicAttackTrigger();
        }
        else
        {
            Debug.LogWarning("<color=red>[경고] 부모 오브젝트에서 PlayerInventory를 찾을 수 없습니다!</color>");
        }
    }
    public void TriggerAttackEvent()
    {
        if (m_Player is EnforcerController enforcer)
        {
            enforcer.TriggerAttackEvent();
        }
        else
        {
            Debug.LogWarning("<color=red>[경고] 현재 캐릭터가 EnforcerController가 아닙니다!</color>");
        }
    }

}
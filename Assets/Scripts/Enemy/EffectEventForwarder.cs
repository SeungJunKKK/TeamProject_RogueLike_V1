using UnityEngine;

public class EffectEventForwarder : MonoBehaviour
{
    private FlyingEnemyAI m_ParentAI;

    private void Awake()
    {
        m_ParentAI = GetComponentInParent<FlyingEnemyAI>();
    }

    public void TriggerAttack()
    {
        if (m_ParentAI != null)
        {
            m_ParentAI.TriggerAttack();
        }
    }

    public void FinishAttack()
    {
        if (m_ParentAI != null)
        {
            m_ParentAI.FinishAttack();
        }
    }
}
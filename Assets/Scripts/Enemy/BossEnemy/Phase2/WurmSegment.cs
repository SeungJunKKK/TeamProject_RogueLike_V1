using UnityEngine;

public class WurmSegment : MonoBehaviour, IDamageable
{
    [SerializeField] private GildedWurmBase m_ParentWurm;

    public void Init(GildedWurmBase parentWurm)
    {
        m_ParentWurm = parentWurm;
    }

    public void TakeDamage(DamageInfo info)
    {
        if (m_ParentWurm != null)
        {
            DamageInfo adjustedInfo = info;
            adjustedInfo.HitPoint = transform.position;
            m_ParentWurm.TakeDamage(adjustedInfo);
        }
    }
}
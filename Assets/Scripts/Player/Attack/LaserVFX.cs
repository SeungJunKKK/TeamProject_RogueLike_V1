using System.Collections;
using UnityEngine;

public class LaserVFX : MonoBehaviour, IPoolable
{
    [Header("이펙트 유지 시간")]
    public float Duration = 0.1f; 

    private PooledObject m_Pooled;

    public void OnSpawn()
    {
        StartCoroutine(AutoReturnRoutine());
    }

    public void OnDespawn()
    {
        StopAllCoroutines();
    }

    private IEnumerator AutoReturnRoutine()
    {
        yield return new WaitForSeconds(Duration);
        m_Pooled ??= GetComponent<PooledObject>();
        m_Pooled.Return();
    }
}
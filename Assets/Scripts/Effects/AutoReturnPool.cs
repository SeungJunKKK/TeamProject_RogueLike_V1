using UnityEngine;
using System.Collections;

//이펙트 전용
public class AutoReturnPool : MonoBehaviour, IPoolable 
{
    public float returnTime = 0.5f; // 이펙트 지속 시간
    private Coroutine m_ReturnCoroutine;

    public void OnSpawn()
    {
        if (m_ReturnCoroutine != null) StopCoroutine(m_ReturnCoroutine);
        m_ReturnCoroutine = StartCoroutine(ReturnRoutine());
    }

    public void OnDespawn()
    {
        if (m_ReturnCoroutine != null)
        {
            StopCoroutine(m_ReturnCoroutine);
            m_ReturnCoroutine = null;
        }
    }

    private IEnumerator ReturnRoutine()
    {
        yield return new WaitForSeconds(returnTime);

        // PooledObject 컴포넌트를 찾아 Return() 호출
        if (TryGetComponent(out PooledObject pooledObj))
        {
            pooledObj.Return();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
using UnityEngine;
using System.Collections;

public class GrenadeDustEffect : MonoBehaviour, IPoolable
{
    private SpriteRenderer m_Sr;
    private PooledObject m_PooledObj;
    private Vector3 m_InitialScale;

    private void Awake()
    {
        m_Sr = GetComponent<SpriteRenderer>();
        m_PooledObj = GetComponent<PooledObject>();
        m_InitialScale = transform.localScale;
    }

    public void OnSpawn() 
    {
        m_Sr.color = new Color(1f, 1f, 1f, 0.7f); 
        transform.localScale = m_InitialScale;
        StartCoroutine(FadeOutAndReturn());
    }

    public void OnDespawn() 
    {
        StopAllCoroutines();
    }

    private IEnumerator FadeOutAndReturn()
    {
        float duration = 0.3f; 
        float time = 0;

        Vector2 randomDrift = new Vector2(Random.Range(-1f, 1f), Random.Range(0.2f, 1.0f)) * 2f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;

            m_Sr.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.7f, 0f, progress));
            transform.localScale = Vector3.Lerp(m_InitialScale, m_InitialScale * 0.3f, progress);
            transform.Translate(randomDrift * Time.deltaTime);

            yield return null;
        }

        if (m_PooledObj != null) m_PooledObj.Return();
    }
}
using UnityEngine;

public class BulletTest : MonoBehaviour, IPoolable
{
    private float m_Timer;

    [Header("Test")]
    public float Speed = 10.0f;
    public void OnDespawn()
    {
        Debug.Log($"prefab 삭제");
    }
    public void OnSpawn()
    {
        m_Timer = 0.0f;
    }

    private void Update()
    {
        m_Timer += Time.deltaTime;
        transform.Translate(Vector3.right * Speed * Time.deltaTime);

        if (m_Timer >= 3.0f)
        {
            GetComponent<PooledObject>().Return();
        }
    }
}

using UnityEngine;

// 힐링 이펙트 프리팹(VFX_HealEffect)에 부착할 이동 스크립트
public class ScatterEffect : MonoBehaviour, IPoolable
{
    public float moveSpeed = 1.0f; // 퍼져나가는 속도
    private Vector3 m_Direction;   // 퍼져나갈 무작위 방향

    public void OnSpawn()
    {
        m_Direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
    }

    public void OnDespawn()
    {
    }

    private void Update()
    {
        transform.Translate(m_Direction * moveSpeed * Time.deltaTime, Space.World);
    }
}
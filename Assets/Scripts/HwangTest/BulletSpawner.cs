using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BulletSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject m_BulletPrefab;

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCoroutine(BulletSpawn());
        }
    }

    private IEnumerator BulletSpawn()
    {
        Vector2 pos = transform.position;
        for (int i = 0; i < 100; ++i)
        {
            PoolManager.Instance.Get(m_BulletPrefab, pos + Random.insideUnitCircle, Quaternion.identity);
            yield return new WaitForSeconds(0.1f);
        }
    }
}

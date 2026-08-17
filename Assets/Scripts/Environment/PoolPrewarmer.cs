using System.Collections;
using UnityEngine;

public class PoolPrewarmer : MonoBehaviour
{
    [System.Serializable]
    public struct PrewarmEntry
    {
        public GameObject prefab;
        public int count;
    }

    [SerializeField] private PrewarmEntry[] m_Entries;

    public IEnumerator PrewarmRoutine()
    {
        if (PoolManager.Instance == null)
        {
            yield break;
        }

        foreach (PrewarmEntry entry in m_Entries)
        {
            for (int i = 0; i < entry.count; i++)
            {
                PoolManager.Instance.Prewarm(entry.prefab, 1);
                yield return null;   // 한 개당 한 프레임 → 분산
            }
        }
    }
}

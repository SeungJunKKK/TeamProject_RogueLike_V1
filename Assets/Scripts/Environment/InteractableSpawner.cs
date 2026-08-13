using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class InteractableSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct ChestSpawnInfo
    {
        public GameObject prefab;
        public int count;
    }

    [SerializeField]
    private Tilemap[] m_GroundTilemaps;

    [Header("등급 상자")]
    [SerializeField] private ChestSpawnInfo[] m_ChestTypes;

    [Header("무한 상자")]
    [SerializeField] private GameObject m_InfiniteChestPrefab;
    [SerializeField] private int m_InfiniteChestCount = 1;

    [Header("텔레포터")]
    [SerializeField] private GameObject m_TeleporterPrefab;
    [SerializeField] private int m_TeleportCount = 1;

    [Header("배치 거리")]
    [SerializeField] private float m_MinDistance = 3f;

    private void Start()
    {
        List<Vector2> valid = ScanValidPositions();

        // 텔레포터
        for (int i = 0; i < m_TeleportCount && valid.Count > 0; i++)
        {
            PlaceRandom(m_TeleporterPrefab, valid);
        }

        // 무한 상자
        for (int i = 0; i < m_InfiniteChestCount && valid.Count > 0; i++)
        {
            PlaceRandom(m_InfiniteChestPrefab, valid);
        }

        // 등급상자
        foreach (var info in m_ChestTypes)
        {
            if (info.prefab == null)
            {
                continue;
            }
            for (int i = 0; i < info.count && valid.Count > 0; i++)
            {
                PlaceRandom(info.prefab, valid);
            }
        }
    }

    private void PlaceRandom(GameObject prefab, List<Vector2> positions)
    {
        if (positions.Count == 0)  
        {
            return;
        }

        int index = Random.Range(0, positions.Count);
        Vector2 pos = positions[index];
        Instantiate(prefab, pos, Quaternion.identity);
        positions.RemoveAll(p => Vector2.Distance(p, pos) < m_MinDistance);
    }

    private List<Vector2> ScanValidPositions()
    {
        List<Vector2> points = new List<Vector2>();
        
        foreach (var tilemap in m_GroundTilemaps)
        {
            if (tilemap == null)
            {
                continue;
            }

            BoundsInt bounds = tilemap.cellBounds;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int cell = new Vector3Int(x, y, 0);
                    Vector3Int above = new Vector3Int(x, y + 1, 0);

                    if (tilemap.HasTile(cell) && !tilemap.HasTile(above))
                    {
                        points.Add(tilemap.GetCellCenterWorld(above));
                    }
                }
            }
        }

        return points;
    }
}

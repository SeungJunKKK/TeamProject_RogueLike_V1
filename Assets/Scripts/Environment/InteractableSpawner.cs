using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;

public class InteractableSpawner : MonoBehaviour
{
    [SerializeField]
    private Tilemap[] m_GroundTilemaps;

    [Header("배치 설정")]
    [SerializeField]
    private int m_ChestCount = 12;
    [SerializeField]
    private int m_TeleportCount = 1;
    [SerializeField]
    private int m_InfiniteChestCount = 1;

    [Header("프리팹 연결")]
    [SerializeField]
    private GameObject m_TeleporterPrefab;
    [SerializeField]
    private GameObject m_ChestPrefab;
    [SerializeField]
    private GameObject m_InfiniteChestPrefab;

    [SerializeField]
    private float m_MinDistance = 3f;

    private void Start()
    {
        List<Vector2> valid = ScanValidPositions();

        if (valid.Count <  m_ChestCount + m_TeleportCount)
        {
            Debug.LogWarning($"[InteractableSpawner] 배치 공간 부족 " +
                $"(필요: {m_ChestCount + m_TeleportCount}, 있음: {valid.Count})");
        }

        if (m_TeleportCount > 0)
        {
            PlaceRandom(m_TeleporterPrefab, valid);
        }

        if (m_InfiniteChestCount > 0)
        {
            for (int i = 0; i < m_InfiniteChestCount && valid.Count > 0; ++i)
            {
                PlaceRandom(m_InfiniteChestPrefab, valid);
            }
        }

        if (m_ChestCount > 0)
        {
            for (int i = 0; i < m_ChestCount && valid.Count > 0; ++i)
            {
                PlaceRandom(m_ChestPrefab, valid);
            }
        }
    }

    private void PlaceRandom(GameObject prefab, List<Vector2> positions)
    {
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

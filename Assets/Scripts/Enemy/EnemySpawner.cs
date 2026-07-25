using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("필수 연결 요소")]
    public Tilemap GroundTilemap;
    public Transform Player;
    public GameObject[] EnemyPrefabs;

    [Header("스폰 설정")]
    public float SpawnInterval = 3f;
    public float MinDistanceFromPlayer = 10f;
    public int MaxSpawnAttempts = 10;
    public int MaxEnemyCount = 20;

    [Header("스폰 안전장치")]
    public float SpawnClearanceRadius = 1f;
    public LayerMask EnemyLayer;

    // 스캔 완료 후 배열로 관리하여 메모리/접근 속도 최적화
    private Vector2[] m_ValidSpawnPoints;
    private HashSet<GameObject> m_ActiveEnemies = new HashSet<GameObject>();
    private Collider2D[] m_OverlapBuffer = new Collider2D[1];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        } 
    }

    private void Start()
    {
        if (GroundTilemap == null || Player == null || EnemyPrefabs == null || EnemyPrefabs.Length == 0)
        {
            Debug.LogError("[EnemySpawner] 필수 요소가 연결되지 않았습니다.");
            return;
        }

        ScanTilemapForSpawnPoints();

        if (m_ValidSpawnPoints == null || m_ValidSpawnPoints.Length == 0)
        {
            Debug.LogError("[EnemySpawner] 스폰 가능한 위치를 찾지 못했습니다.");
            return;
        }

        if (PoolManager.Instance == null)
        {
            Debug.LogError("[EnemySpawner] PoolManager를 찾을 수 없습니다.");
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    private void ScanTilemapForSpawnPoints()
    {
        List<Vector2> tempPoints = new List<Vector2>();
        BoundsInt bounds = GroundTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int currentCell = new Vector3Int(x, y, 0);
                if (GroundTilemap.HasTile(currentCell))
                {
                    Vector3Int aboveCell = new Vector3Int(x, y + 1, 0);
                    if (!GroundTilemap.HasTile(aboveCell))
                    {
                        tempPoints.Add(GroundTilemap.GetCellCenterWorld(aboveCell));
                    }
                }
            }
        }

        // List를 Vector2[] 배열로 변환하여 캐싱
        m_ValidSpawnPoints = tempPoints.ToArray();
        Debug.Log($"<color=green>[EnemySpawner]</color> 스캔 완료! {m_ValidSpawnPoints.Length}개의 위치 확보.");
    }

    public void UnregisterEnemy(GameObject enemy)
    {
        m_ActiveEnemies.Remove(enemy);
    }

    private IEnumerator SpawnRoutine()
    {
        while (Player != null)
        {
            yield return new WaitForSeconds(SpawnInterval);

            if (m_ActiveEnemies.Count < MaxEnemyCount)
            {
                SpawnEnemy();
            }
        }
    }

    private void SpawnEnemy()
    {
        if (Player == null)
        {
            return;
        }

        for (int i = 0; i < MaxSpawnAttempts; i++)
        {
            Vector2 testPoint = m_ValidSpawnPoints[Random.Range(0, m_ValidSpawnPoints.Length)];

            if (Vector2.Distance(testPoint, Player.position) < MinDistanceFromPlayer)
            {
                continue;
            }

            if (Physics2D.OverlapCircleNonAlloc(testPoint, SpawnClearanceRadius, m_OverlapBuffer, EnemyLayer) == 0)
            {
                GameObject enemyObj = PoolManager.Instance.Get(EnemyPrefabs[Random.Range(0, EnemyPrefabs.Length)], testPoint, Quaternion.identity);

                if (enemyObj == null)
                {
                    Debug.LogError("[EnemySpawner] PoolManager.Get()이 null을 반환했습니다.");
                    return;
                }

                if (!enemyObj.TryGetComponent(out EnemyBase enemyBase))
                {
                    Debug.LogError($"[EnemySpawner] {enemyObj.name}에 EnemyBase가 없습니다.");
                    return;
                }

                enemyBase.SetTarget(Player);
                m_ActiveEnemies.Add(enemyObj);
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (m_ValidSpawnPoints == null)
        {
            return; 
        }

        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        int drawCount = 0;
        foreach (Vector2 point in m_ValidSpawnPoints)
        {
            if (drawCount++ >= 100)
            {
                break;
            }
            Gizmos.DrawCube(point, new Vector3(0.5f, 0.5f, 0.5f));
        }
    }
}
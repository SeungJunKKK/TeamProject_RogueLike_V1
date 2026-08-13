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

    private Vector2[] m_ValidSpawnPoints;
    private HashSet<GameObject> m_ActiveEnemies = new HashSet<GameObject>();
    private Collider2D[] m_OverlapBuffer = new Collider2D[1];

    // 외부 조회용 프로퍼티 추가 
    public int ActiveEnemyCount => m_ActiveEnemies.Count;

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
        if (GroundTilemap == null || EnemyPrefabs == null || EnemyPrefabs.Length == 0) 
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

        m_ValidSpawnPoints = tempPoints.ToArray();
    }

    public void UnregisterEnemy(GameObject enemy)
    {
        m_ActiveEnemies.Remove(enemy);
    }

    private IEnumerator SpawnRoutine()
    {
        while (true) 
        {
            if (Player == null)
            {
                if (GameManager.Instance != null && GameManager.Instance.CurrentPlayer != null)
                {
                    Player = GameManager.Instance.CurrentPlayer.transform;
                    Debug.Log("<color=green>[EnemySpawner] 타겟 플레이어 확인 완료! 스폰 루틴 시작.</color>");
                }
                else
                {
                yield return null;
                continue;
                }
            }
            yield return new WaitForSeconds(SpawnInterval); 

            if (m_ActiveEnemies.Count < MaxEnemyCount) 
            {
            SpawnEnemy(); 
            }
        }
    }

    private void SpawnEnemy()
    {
        if (Player == null || EnemyPrefabs.Length == 0) return;

        // 랜덤으로 프리팹 선택
        GameObject selectedPrefab = EnemyPrefabs[Random.Range(0, EnemyPrefabs.Length)];

        Vector2 spawnPoint = Vector2.zero;
        bool foundValidSpawn = false;
        bool isFlyingEnemy = false;

        for (int i = 0; i < MaxSpawnAttempts; i++)
        {
            Vector2 testPoint = m_ValidSpawnPoints[Random.Range(0, m_ValidSpawnPoints.Length)];

            if (Vector2.Distance(testPoint, Player.position) < MinDistanceFromPlayer)
            {
                continue;
            }

            ContactFilter2D filter = new ContactFilter2D();
            filter.useLayerMask = true;
            filter.layerMask = EnemyLayer;

            if (Physics2D.OverlapCircle(testPoint, SpawnClearanceRadius, filter, m_OverlapBuffer) == 0)
            {
                spawnPoint = testPoint;
                foundValidSpawn = true;
                break;
            }
        }

        // 유효한 스폰 자리를 찾았을 때 생성 실행
        if (foundValidSpawn)
        {
            GameObject enemyObj = PoolManager.Instance.Get(selectedPrefab, spawnPoint, Quaternion.identity);

            if (enemyObj == null)
            {
                Debug.LogError("[EnemySpawner] PoolManager.Get()이 null을 반환했습니다.");
                return;
            }

            if (enemyObj.layer == LayerMask.NameToLayer("FlyingEnemy"))
            {
                // 공중 몬스터라면 바닥이 아니라 플레이어 주변 허공 좌표로 위치 재배치
                float randomX = Random.Range(-12f, 12f);
                float randomY = Random.Range(3f, 7f);
                enemyObj.transform.position = (Vector2)Player.position + new Vector2(randomX, randomY);
            }

            if (!enemyObj.TryGetComponent(out EnemyBase enemyBase))
            {
                Debug.LogError($"[EnemySpawner] {enemyObj.name}에 EnemyBase가 없습니다.");
                return;
            }

            enemyBase.SetTarget(Player);
            m_ActiveEnemies.Add(enemyObj);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (m_ValidSpawnPoints == null) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        int drawCount = 0;
        foreach (Vector2 point in m_ValidSpawnPoints)
        {
            if (drawCount++ >= 100) break;
            Gizmos.DrawCube(point, new Vector3(0.5f, 0.5f, 0.5f));
        }
    }
}
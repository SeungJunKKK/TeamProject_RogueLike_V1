using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("필수 연결 요소")]
    [Tooltip("지형 Tilemap 등록")]
    [SerializeField] private Tilemap[] m_SpawnTilemaps;

    public Transform Player;
    public GameObject[] EnemyPrefabs;

    [Header("스폰 설정")]
    public float SpawnInterval = 3f;
    public float MinDistanceFromPlayer = 10f;
    public int MaxSpawnAttempts = 10;
    public int MaxEnemyCount = 20;

    [Header("공중 몬스터 스폰 설정")]
    [SerializeField] private float m_AirSpawnMinHeight = 3f;
    [SerializeField] private float m_AirSpawnMaxHeight = 7f;
    [SerializeField] private float m_AirSpawnHorizontalRange = 12f;

    [Header("스폰 안전장치")]
    public float SpawnClearanceRadius = 1f;
    public LayerMask EnemyLayer;

    private Vector2[] m_ValidSpawnPoints;

    private readonly HashSet<GameObject> m_ActiveEnemies =
        new HashSet<GameObject>();

    private readonly Collider2D[] m_OverlapBuffer =
        new Collider2D[1];

    private bool m_SpawningEnabled = true;

    public int ActiveEnemyCount => m_ActiveEnemies.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        EventBus.Subscribe<TeleporterStateChangedEvent>(OnTeleporterState);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            EventBus.Unsubscribe<TeleporterStateChangedEvent>(OnTeleporterState);
        }
    }

    private void Start()
    {
        if (m_SpawnTilemaps == null ||
            m_SpawnTilemaps.Length == 0 ||
            EnemyPrefabs == null ||
            EnemyPrefabs.Length == 0)
        {
            Debug.LogError(
                "[EnemySpawner] 필수 요소(타일맵 또는 프리팹)가 연결되지 않았습니다."
            );

            return;
        }

        ScanTilemapForSpawnPoints();

        if (m_ValidSpawnPoints == null ||
            m_ValidSpawnPoints.Length == 0)
        {
            Debug.LogError(
                "[EnemySpawner] 스폰 가능한 위치를 찾지 못했습니다."
            );

            return;
        }

        if (PoolManager.Instance == null)
        {
            Debug.LogError(
                "[EnemySpawner] PoolManager를 찾을 수 없습니다."
            );

            return;
        }

    //Debug.Log(
    //$"[EnemySpawner] 스폰 시스템 시작 | " +
    //$"PoolManager=OK | " +
    //$"SpawnPoints={m_ValidSpawnPoints.Length} | " +
    //$"MaxEnemyCount={MaxEnemyCount} | " +
    //$"SpawnInterval={SpawnInterval}"
    //        );
                    
        StartCoroutine(SpawnRoutine());
    }

    // ==========================================
    // [Module: Spawn Point Scanning]
    // ==========================================

    private void ScanTilemapForSpawnPoints()
    {

    //    Debug.Log(
    //    $"[EnemySpawner] 실행 객체: {gameObject.name} / " +
    //    $"Scene: {gameObject.scene.name} / " +
    //    $"Tilemap 배열 크기: {m_SpawnTilemaps?.Length ?? -1}"
    //);

        List<Vector2> tempPoints = new List<Vector2>();

        foreach (Tilemap tilemap in m_SpawnTilemaps)
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
                    Vector3Int currentCell =
                        new Vector3Int(x, y, 0);

                    if (!tilemap.HasTile(currentCell))
                    {
                        continue;
                    }

                    //Debug.Log(
                    //    $"[EnemySpawner] 연결된 Tilemap: {tilemap.name}"
                    //);

                    Vector3Int aboveCell =
                        new Vector3Int(x, y + 1, 0);

                    // 현재 타일 위가 비어 있으면
                    // 해당 위치를 지상 몬스터 스폰 후보로 등록
                    if (!tilemap.HasTile(aboveCell))
                    {
                        tempPoints.Add(
                            tilemap.GetCellCenterWorld(aboveCell)
                        );
                    }
                }
            }
        }

        m_ValidSpawnPoints = tempPoints.ToArray();

        //Debug.Log(
        //    $"[EnemySpawner] 총 {m_ValidSpawnPoints.Length}개의 " +
        //    $"지상 스폰 포인트가 스캔되었습니다."
        //);
    }

    // ==========================================
    // [Module: Enemy Registration]
    // ==========================================

    public void UnregisterEnemy(GameObject enemy)
    {
        m_ActiveEnemies.Remove(enemy);
    }

    // ==========================================
    // [Module: Spawn Routine]
    // ==========================================

    private IEnumerator SpawnRoutine()
    {
        //Debug.Log("[EnemySpawner] SpawnRoutine 시작");

        while (true)
        {
            if (Player == null)
            {
                if (GameManager.Instance != null &&
                    GameManager.Instance.CurrentPlayer != null)
                {
                    Player =
                        GameManager.Instance.CurrentPlayer.transform;

                    //Debug.Log(
                    //    "<color=green>" +
                    //    "[EnemySpawner] 타겟 플레이어 확인 완료!" +
                    //    "</color>"
                    //);
                }
                else
                {
                    yield return null;
                    continue;
                }
            }

            yield return new WaitForSeconds(SpawnInterval);

            //Debug.Log(
            //    $"[EnemySpawner] Spawn 검사 | " +
            //    $"Enabled={m_SpawningEnabled} | " +
            //    $"Active={m_ActiveEnemies.Count} / {MaxEnemyCount}"
            //);

            if (m_SpawningEnabled &&
                m_ActiveEnemies.Count < MaxEnemyCount)
            {
               // Debug.Log("[EnemySpawner] SpawnEnemy 실행");
                SpawnEnemy();
            }
        }
    }

    // ==========================================
    // [Module: Enemy Spawn]
    // ==========================================

    private void SpawnEnemy()
    {
        //Debug.Log("[EnemySpawner] SpawnEnemy 호출");
        if (Player == null ||
            EnemyPrefabs == null ||
            EnemyPrefabs.Length == 0)
        {
            return;
        }

        // 랜덤으로 몬스터 프리팹 선택
        GameObject selectedPrefab =
            EnemyPrefabs[
                Random.Range(0, EnemyPrefabs.Length)
            ];

        Vector2 spawnPoint = Vector2.zero;
        bool foundValidSpawn = false;

        // ------------------------------------------
        // 지상 스폰 위치 탐색
        // ------------------------------------------

        for (int i = 0; i < MaxSpawnAttempts; i++)
        {
            Vector2 testPoint =
                m_ValidSpawnPoints[
                    Random.Range(
                        0,
                        m_ValidSpawnPoints.Length
                    )
                ];

            // 플레이어와 너무 가까우면 제외
            if (Vector2.Distance(
                    testPoint,
                    Player.position) < MinDistanceFromPlayer)
            {
                continue;
            }

            ContactFilter2D filter =
                new ContactFilter2D();

            filter.useLayerMask = true;
            filter.layerMask = EnemyLayer;

            // 다른 몬스터와 겹치는 위치인지 확인
            if (Physics2D.OverlapCircle(
                    testPoint,
                    SpawnClearanceRadius,
                    filter,
                    m_OverlapBuffer) == 0)
            {
                spawnPoint = testPoint;
                foundValidSpawn = true;
                break;
            }
        }

        if (!foundValidSpawn)
        {
            return;
        }

        // ------------------------------------------
        // Pool에서 몬스터 가져오기
        // ------------------------------------------

        GameObject enemyObj =
            PoolManager.Instance.Get(
                selectedPrefab,
                spawnPoint,
                Quaternion.identity
            );

        if (enemyObj == null)
        {
            Debug.LogError(
                "[EnemySpawner] PoolManager.Get()이 null을 반환했습니다."
            );

            return;
        }

        // ------------------------------------------
        // 공중 몬스터 처리
        // ------------------------------------------

        if (enemyObj.layer ==
            LayerMask.NameToLayer("FlyingEnemy"))
        {
            float randomX =
                Random.Range(
                    -m_AirSpawnHorizontalRange,
                    m_AirSpawnHorizontalRange
                );

            float randomY =
                Random.Range(
                    m_AirSpawnMinHeight,
                    m_AirSpawnMaxHeight
                );

            enemyObj.transform.position =
                (Vector2)Player.position +
                new Vector2(randomX, randomY);
        }

        // ------------------------------------------
        // EnemyBase 확인 및 Target 주입
        // ------------------------------------------

        if (!enemyObj.TryGetComponent(
                out EnemyBase enemyBase))
        {
            Debug.LogError(
                $"[EnemySpawner] {enemyObj.name}에 " +
                "EnemyBase가 없습니다."
            );

            return;
        }

        enemyBase.SetTarget(Player);

        // 활성 몬스터 등록
        m_ActiveEnemies.Add(enemyObj);
    }

    // ==========================================
    // [Module: Debug]
    // ==========================================

    private void OnDrawGizmosSelected()
    {
        if (m_ValidSpawnPoints == null)
        {
            return;
        }

        Gizmos.color =
            new Color(0f, 1f, 0f, 0.3f);

        int drawCount = 0;

        foreach (Vector2 point in m_ValidSpawnPoints)
        {
            if (drawCount++ >= 100)
            {
                break;
            }

            Gizmos.DrawCube(
                point,
                new Vector3(0.5f, 0.5f, 0.5f)
            );
        }
    }

    private void OnTeleporterState(TeleporterStateChangedEvent e)
    {
        m_SpawningEnabled =
            (e.State == ETeleporterState.Idle) ||
            (e.State == ETeleporterState.Charging);

        //Debug.Log(
        //    $"[EnemySpawner] Teleporter 상태 변경 | " +
        //    $"State={e.State} | " +
        //    $"SpawningEnabled={m_SpawningEnabled}"
        //);
    }
}
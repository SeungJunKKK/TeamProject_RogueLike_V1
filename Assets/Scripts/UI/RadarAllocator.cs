using UnityEngine;
using System.Collections.Generic;

public class RadarAllocator : MonoBehaviour
{
    [Header("Targeting Settings")]
    [Tooltip("플레이어 주변을 도는 레이더의 반지름")]
    [SerializeField] private float m_Radius = 1.5f;
    [Tooltip("타겟을 갱신하는 주기 (최적화용)")]
    [SerializeField] private float m_SearchInterval = 0.2f;

    [Header("Arrow Sprites")]
    [SerializeField] private Sprite m_MonsterArrowSprite;   
    [SerializeField] private Sprite m_TeleporterArrowSprite; 

    private SpriteRenderer m_SpriteRenderer;
    private Transform m_Player;
    private Transform m_CurrentTarget;
    private Transform m_Teleporter;

    private Camera m_MainCamera;

    private float m_SearchTimer = 0f;
    private bool m_IsActive = false;
    //private bool m_IsAllMonstersDead = false;

    private void Awake()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        m_SpriteRenderer.enabled = false; 
    }
    private void OnEnable()
    {
        EventBus.Subscribe<TeleporterStateChangedEvent>(OnTeleporterStateChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<TeleporterStateChangedEvent>(OnTeleporterStateChanged);
    }

    public void Init(Transform playerTransform, Transform teleporterTransform)
    {
        m_Player = playerTransform;
        m_Teleporter = teleporterTransform;

        m_MainCamera = Camera.main;

        //transform.SetParent(m_Player);
        //transform.localPosition = Vector3.zero;

        DontDestroyOnLoad(gameObject);
    }

    private void OnTeleporterStateChanged(TeleporterStateChangedEvent e)
    {
        if (e.State == ETeleporterState.Charging ||
            e.State == ETeleporterState.WaitingForClear||
            e.State== ETeleporterState.Cleared)
        {
            m_IsActive = true;
            m_SpriteRenderer.enabled = true;
            m_SearchTimer = m_SearchInterval; 
        }
        else 
        {
            m_IsActive = false;
            m_SpriteRenderer.enabled = false;
        }
    }

    public void ActivateRadar()
    {
        m_IsActive = true;
        m_SpriteRenderer.enabled = true;
        m_SearchTimer = m_SearchInterval; 
    }

    public void DeactivateRadar()
    {
        m_IsActive = false;
        m_SpriteRenderer.enabled = false;
    }

    private void Update()
    {
        if (m_Player == null)
        {
            Destroy(gameObject);
            return;
        }

        if (!m_IsActive)
        {
            return;
        }

        m_SearchTimer += Time.deltaTime;

        if (m_SearchTimer >= m_SearchInterval)
        {
            m_SearchTimer = 0f;
            FindNearestTarget();
        }

        if (m_CurrentTarget != null && m_SpriteRenderer.enabled)
        {
            UpdateRadarTransform();
        }
    }

    private void FindNearestTarget()
    {
        if (EnemySpawner.Instance == null)
        {
            return;
        }

        HashSet<GameObject> activeEnemies = EnemySpawner.Instance.GetActiveEnemies();

        if (m_MainCamera == null)
        {
            m_MainCamera = Camera.main;
            if (m_MainCamera == null)
            {
                return;
            }
        }

        if (activeEnemies.Count == 0)
        {
            if (m_Teleporter == null)
            {
                Teleporter stageTeleporter = FindAnyObjectByType<Teleporter>();
                if (stageTeleporter != null)
                {
                    m_Teleporter = stageTeleporter.transform;
                }
            }

            m_CurrentTarget = m_Teleporter;

            if (m_CurrentTarget == null)
            {
                m_SpriteRenderer.enabled = false;
                return;
            }

            m_SpriteRenderer.sprite = m_TeleporterArrowSprite;
            m_SpriteRenderer.enabled = true;

            return;
        }
        m_SpriteRenderer.sprite = m_MonsterArrowSprite;

        float minDistanceSqr = Mathf.Infinity;
        Transform nearestOffScreenMonster = null;

        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy == null || !enemy.activeInHierarchy)
            {
                continue;
            }
            Vector3 viewportPos = m_MainCamera.WorldToViewportPoint(enemy.transform.position);
            bool isVisibleOnScreen = viewportPos.x >= 0f && viewportPos.x <= 1f && viewportPos.y >= 0f && viewportPos.y <= 1f;

            if (isVisibleOnScreen)
            {
                continue;
            }

            Vector2 directionToTarget = enemy.transform.position - m_Player.position;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if (dSqrToTarget < minDistanceSqr)
            {
                minDistanceSqr = dSqrToTarget;
                nearestOffScreenMonster = enemy.transform;
            }
        }

        m_CurrentTarget = nearestOffScreenMonster;

        m_SpriteRenderer.enabled = (m_CurrentTarget != null);
    }
    private void UpdateRadarTransform()
    {
        Vector2 direction = (m_CurrentTarget.position - m_Player.position).normalized;

        transform.position = (Vector2)m_Player.position + (direction * m_Radius);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetTeleporter(Transform newTeleporter)
    {
        m_Teleporter = newTeleporter;

        DeactivateRadar();
    }
}
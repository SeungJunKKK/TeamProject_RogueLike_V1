using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldEffectUI : MonoBehaviour
{
    public static GoldEffectUI Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private GameObject goldEffectPrefab; // 골드 이펙트 프리팹
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private Transform poolParent;

    [Header("Animation Settings")]
    [SerializeField] private float moveDistance = 1.0f; // 위로 떠오르는 거리
    [SerializeField] private float duration = 0.8f; // 연출 지속 시간
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.5f, 0f); // 몬스터 중심 대비 오프셋

    private Queue<GameObject> poolQueue = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePool();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<MonsterDiedEvent>(OnMonsterDied);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<MonsterDiedEvent>(OnMonsterDied);
    }

    private void InitializePool()
    {
        if (goldEffectPrefab == null) return;
        if (poolParent == null) poolParent = transform;

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(goldEffectPrefab, poolParent);
            obj.SetActive(false);
            poolQueue.Enqueue(obj);
        }
    }

    private void OnMonsterDied(MonsterDiedEvent e)
    {
        if (e.Gold <= 0) return;
        PlayGoldEffect((Vector3)e.Position + spawnOffset);
    }

    public void PlayGoldEffect(Vector3 worldPosition)
    {
        GameObject effectObj = GetFromPool();
        effectObj.transform.position = worldPosition;
        effectObj.SetActive(true);

        StartCoroutine(AnimateAndReturn(effectObj));
    }

    private GameObject GetFromPool()
    {
        if (poolQueue.Count > 0)
        {
            return poolQueue.Dequeue();
        }
        else
        {
            return Instantiate(goldEffectPrefab, poolParent);
        }
    }

    private void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        poolQueue.Enqueue(obj);
    }

    private IEnumerator AnimateAndReturn(GameObject targetObj)
    {
        Vector3 startPos = targetObj.transform.position;
        Vector3 endPos = startPos + Vector3.up * moveDistance;

        SpriteRenderer spriteRenderer = targetObj.GetComponentInChildren<SpriteRenderer>();
        Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // 위로 이동 연출
            targetObj.transform.position = Vector3.Lerp(startPos, endPos, t);

            // 페이드 아웃
            if (spriteRenderer != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, t);
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            yield return null;
        }

        // 투명도 원복 후 풀 반환
        if (spriteRenderer != null)
        {
            spriteRenderer.color = startColor;
        }

        ReturnToPool(targetObj);
    }
}
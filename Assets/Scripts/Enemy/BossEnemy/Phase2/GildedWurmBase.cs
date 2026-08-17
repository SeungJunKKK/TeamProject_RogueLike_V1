using UnityEngine;
using System.Collections;

public abstract class GildedWurmBase : EnemyBase
{
    [Header("Gilded Wurm Common Settings")]
    [SerializeField] protected Transform m_Player;
    [SerializeField] protected float m_MoveSpeed = 3f;
    protected bool m_IsEmerging = true;

    protected override bool CanReceiveKnockback => false;

    public override void SetTarget(Transform target)
    {
        m_Player = target;
    }

    public virtual void Setup(Transform player)
    {
        m_Player = player;
        StartCoroutine(BurrowRoutine());
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        WurmSegmentMover.ClearHistory();

        WurmSegmentMover[] movers = GetComponentsInChildren<WurmSegmentMover>();
        for (int i = 0; i < movers.Length; i++)
        {
            movers[i].Init(i + 1);
        }

        WurmSegment[] segments = GetComponentsInChildren<WurmSegment>();
        foreach (var segment in segments)
        {
            segment.Init(this);
        }
    }

    protected virtual void Update()
    {
        if (!m_IsEmerging)
        {
            WurmSegmentMover.RecordHeadPosition(transform.position);
        }
    }

    protected virtual IEnumerator BurrowRoutine()
    {
        m_IsEmerging = true;
        Vector3 surfacePos = transform.position;
        Vector3 undergroundPos = surfacePos + Vector3.down * 6f;
        transform.position = undergroundPos;

        float elapsed = 0f;
        float duration = 1.2f;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(undergroundPos, surfacePos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = surfacePos;
        m_IsEmerging = false;
    }

    protected override void Die()
    {
        base.Die();
        if (BossBattleController.Instance != null)
        {
            BossBattleController.Instance.OnWurmDied();
        }
    }
}
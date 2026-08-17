using UnityEngine;
using System.Collections.Generic;

public class WurmSegmentMover : MonoBehaviour
{
    private int m_MyIndex = 0;
    private static List<Vector2> s_HeadPositionHistory = new List<Vector2>();
    [SerializeField] private int m_HistoryDelay = 6;

    public void Init(int index)
    {
        m_MyIndex = index;
    }

    public static void RecordHeadPosition(Vector2 newPos)
    {
        if (s_HeadPositionHistory.Count == 0 || Vector2.Distance(s_HeadPositionHistory[0], newPos) > 0.1f)
        {
            s_HeadPositionHistory.Insert(0, newPos);
            if (s_HeadPositionHistory.Count > 500)
            {
                s_HeadPositionHistory.RemoveAt(s_HeadPositionHistory.Count - 1);
            }
        }
    }

    private void Update()
    {
        int targetIndex = m_MyIndex * m_HistoryDelay;

        if (s_HeadPositionHistory.Count > targetIndex)
        {
            transform.position = Vector2.Lerp(transform.position, s_HeadPositionHistory[targetIndex], 25f * Time.deltaTime);
        }
    }

    public static void ClearHistory()
    {
        s_HeadPositionHistory.Clear();
    }
}
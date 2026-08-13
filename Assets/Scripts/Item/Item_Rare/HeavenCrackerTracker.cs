using UnityEngine;

/// <summary>
/// HeavenCrackerTracker는 플레이어의 공격 횟수를 추적하는 MonoBehaviour 클래스입니다.
/// 플레이어 몸에 붙어서 공격 횟수만 기억하고, HeavenCracker 아이템의 효과를 적용하는 데 사용됩니다.
/// </summary>
public class HeavenCrackerTracker : MonoBehaviour
{
    private int m_AttackCount = 0;

    public int IncrementAndGetCount()
    {
        m_AttackCount++;
       // Debug.Log($"<color=lime>[디버그] 2. Tracker 카운트 증가! 현재 누적 타격 수: {m_AttackCount}</color>");
        return m_AttackCount;
    }

    public void ResetCount()
    {
        m_AttackCount = 0;
        //Debug.Log("<color=lime>[디버그] Tracker 카운트가 0으로 초기화되었습니다.</color>");
    }
}

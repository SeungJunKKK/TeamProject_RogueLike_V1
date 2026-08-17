using System.Collections.Generic;
using UnityEngine;

//For Json Utility
[System.Serializable]
public class PlayerSaveData
{
    [Header("Unlock Data")]
    public List<string> UnlockedItems;
    public List<string> UnlockedMonsters;

    [Header("Record Data")]
    public int MaxLevelReached;
    public float LongestSurviveTime;
    public int MaxKillsInOneRun;
    public int MaxBossesKilled;
    public int MaxGoldCollected;
    public int HighestTotalScore;

    // 생성자: 처음 게임을 시작할 때 세팅될 기본값
    public PlayerSaveData()
    {
        UnlockedItems = new List<string>();
        UnlockedMonsters = new List<string>();

        MaxLevelReached = 1;
        LongestSurviveTime = 0f;
        MaxKillsInOneRun = 0;
        MaxBossesKilled = 0;
        MaxGoldCollected = 0;
        HighestTotalScore = 0;
    }
}
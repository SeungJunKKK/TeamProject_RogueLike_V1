using System.Collections.Generic;
using UnityEngine;

public class CurrentRunData
{
    public string ClassName; 
    public int CurrentLevel;
    public float PlayTime;
    public int Kills;
    public int BossesKilled;
    public int ItemsCollected;
    public int GoldCollected;
    public int Purchases;

    public List<string> NewlyUnlockedItems = new List<string>();

    public void ResetRun(string selectedClassName)
    {
        ClassName = selectedClassName;
        CurrentLevel = 1;
        PlayTime = 0f;
        Kills = 0;
        BossesKilled = 0;
        ItemsCollected = 0;
        GoldCollected = 0;
        Purchases = 0;
        NewlyUnlockedItems.Clear();
    }

    public int CalculateTotalScore()
    {
        int timePoints = Mathf.FloorToInt(PlayTime) * 3;
        int goldPoints = GoldCollected * 2;
        int itemPoints = ItemsCollected * 110;
        int purchasePoints = Purchases * 35;


        int levelPoints = CurrentLevel * 500;  // 레벨당 500점
        int killPoints = Kills * 100;          // 킬당 100점
        int bossPoints = BossesKilled * 1000;  // 보스 처치당 1000점

        int total = timePoints + goldPoints + itemPoints + purchasePoints
                    + levelPoints + killPoints + bossPoints;

        return total;
    }
}
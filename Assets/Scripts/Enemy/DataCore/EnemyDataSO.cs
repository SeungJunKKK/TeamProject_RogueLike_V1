using UnityEngine;



public class EnemyDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public string EnemyName;

    [Header("스탯 (레벨 1 기준)")]
    public float BaseHp = 80f;
    public float HpPerLevel = 24f;
    public float BaseDamage = 12f; 

    [Header("보상")]
    public int BaseGold = 2;
    public float BaseExp = 12f;
    public float ExpPerLevel = 3f;

    [Header("사운드 (Addressable 경로)")]
    public string SpawnSoundAddress = "";
    public string AttackSoundAddress = "";
    public string HitSoundAddress = "";
    public string DeathSoundAddress = "";
}
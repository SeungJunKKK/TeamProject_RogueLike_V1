using UnityEngine;

[CreateAssetMenu(fileName = "NewBossData", menuName = "Boss Data/Boss Data SO")]
public class BossDataSO : EnemyDataSO
{
    [Header("기본 정보")]
    public string BossName;

    [Header("사운드 (Addressable 경로 - 최대 4개)")]
    [Tooltip("예: 등장(Spawn) 사운드")]
    public string Sound1Address = "";

    [Tooltip("예: 피격(Hit) 사운드")]
    public string Sound2Address = "";

    [Tooltip("예: 공격(Attack) 사운드")]
    public string Sound3Address = "";

    [Tooltip("예: 사망(Death) 사운드")]
    public string Sound4Address = "";
}
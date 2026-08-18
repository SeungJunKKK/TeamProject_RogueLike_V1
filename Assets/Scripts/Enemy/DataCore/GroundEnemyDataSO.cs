using UnityEngine;

[CreateAssetMenu(fileName = "New Ground Enemy Data", menuName = "Enemy Data/Ground Enemy")]
public class GroundEnemyDataSO : EnemyDataSO
{
    [Header("이동 설정 (Ground)")]
    public float MoveSpeed = 3f;
    public float Acceleration = 15f;
    public float JumpForce = 6f;
    public float JumpCooldown = 0.5f;

    [Header("전투 설정 (Ground)")]
    public float AttackRange = 1.5f;
    public float AttackWindup = 0.5f;
    public float AttackCooldown = 2f;
    [Range(0f, 1f)] public float AttackMoveRatio = 0.3f;

    [Header("센서 & 분리 (Ground)")]
    public float GroundRayLength = 1.5f;
    public float WallRayLength = 0.2f;
    public float SeparationRadius = 0.8f;
    public float SeparationForce = 2.5f;
}
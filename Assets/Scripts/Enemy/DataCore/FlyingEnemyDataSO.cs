using UnityEngine;

[CreateAssetMenu(fileName = "New Flying Enemy Data", menuName = "Enemy Data/Flying Enemy")]
public class FlyingEnemyDataSO : EnemyDataSO
{
    [Header("이동 설정 (Flying)")]
    public float MoveSpeed = 3f;
    public float Acceleration = 5f;
    public float DashRange = 6f;
    public float DashSpeed = 8f;
    public float DashAcceleration = 15f;

    [Header("전투 설정 (Flying)")]
    public float AttackRange = 1.5f;
    public float AttackCooldown = 2f;
    public float AttackWindup = 0.5f;

    [Header("분리 (Flying)")]
    public float SeparationRadius = 1f;
    public float SeparationStrength = 2f;
}
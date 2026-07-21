using UnityEngine;

public struct SceneLoadStartedEvent
{
    public string SceneName;
}

public struct SceneLoadCompletedEvent
{
    public string SceneName;
}

public struct SpawnProjectileEvent
{
    public GameObject ProjectilePrefab;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector2 Direction;
    public float Damage;
    public float Speed;

    public bool IsPiercing;
}
public struct SpawnVFXEvent
{
    public GameObject VFXPrefab;
    public Vector3 Position;
    public Quaternion Rotation;
}
public struct GameStateChangedEvent
{
    public GameState Previous;
    public GameState Current;
}

// 몬스터가 피격 P4가 발행, P3 플로팅 텍스트 구독
public struct MonsterDamagedEvent
{
    public float Amount; // 표시할 데미지 양
    public Vector2 HitPoint; // 텍스트 뜰 위치
    public bool IsCrit; // 크리티컬 여부
}

// 몬스터 사망 P4가 발행, P3 경험치·UI 킬카운트가 구독
public struct MonsterDiedEvent
{
    public float Exp; // 획득 경험치
    public Vector2 Position; // 사망 위치
}

// 플레이어 피격 P1이 발행, UI가 구독
public struct PlayerDamagedEvent
{
    public float Amount;
    public float CurrentHp;
    public float MaxHp;
}

// 플레이어 사망 P1이 발행, UI가 구독
public struct PlayerDiedEvent { }

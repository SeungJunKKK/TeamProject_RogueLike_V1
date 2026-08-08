using UnityEngine;

public struct SceneLoadStartedEvent
{
    public string SceneName;
}

public struct SceneLoadCompletedEvent
{
    public string SceneName;
}

//총알 생성 및 물리적만 담당 
public struct SpawnProjectileEvent
{
    //시각/물리 데이터 
    public string ProjectileAddress;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector2 Direction;
    public float Speed; 
    public bool IsPiercing;
    //===============//데미지 데이터
    public DamageInfo AttackData;     
}


public struct SpawnVFXEvent
{
    public string VFXAddress;
    public Vector3 Position;
    public Quaternion Rotation;
}
public struct GameStateChangedEvent
{
    public EGameState Previous;
    public EGameState Current;
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
    public int Gold;
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

// 어떤 스탯을 올려주는 아이템인지 구분하기 위한 열거형
public enum EStatType
{
    MoveSpeed, Damage, AttackSpeed, MaxHealth, CritChance, CritDamage
}

public struct ItemPickedUpEvent
{
    public string ItemName;      // 예: "군인의 주사기"
    public EStatType TargetStat;  // 예: StatType.AttackSpeed
    public StatModifier Modifier; // 예: (0.15f, PercentAdd) 
}
public struct WeaponFiredEvent
{
    public string SoundAddress;  // 예: "Commando_Shoot_SFX"
    public string EffectAddress; // 예: "MuzzleFlash_FX"
    public Vector3 Position;
}

public struct GoldChangedEvent
{
    public int Current; // 현재 총 골드
    public int Delta;   // 증감량
}

public struct DifficultyChangedEvent
{
    public EDifficultyLevel Level;
    public float Coefficient;
}

public struct TeleporterStateChangedEvent
{
    public ETeleporterState State;
}

// 보스 사망 이벤트
public struct BossDiedEvent
{
    public GameObject Boss;
    public Vector2 Position;
}

// 상호작용 대상 감지
public struct InteractableInRangeEvent
{
    public Vector2 WorldPosition;   // 프롬프트를 띄울 위치 (대상 머리 위 등)
}

public struct InteractableOutOfRangeEvent { }
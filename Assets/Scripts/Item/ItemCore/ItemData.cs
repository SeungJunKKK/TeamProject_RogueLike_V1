using UnityEngine;

public enum ItemTier
{
    Common,     // 하양 (일반)
    UnCommon,   // 초록 (고급)
    Rare,       // 빨강 (희귀)
    Boss,       // 노랑 (보스)
    Use         // 주황 (액티브/사용 아이템)
}

public abstract class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName;
    [TextArea(3, 5)] public string description;
    public Sprite itemIcon;
    public ItemTier tier;

    [Header("설정")]
    [Tooltip("아이템이 중첩 가능한지 여부")]
    public bool isStackable = true;
    [Tooltip("0으로 설정 시 무제한 중첩")]
    public int maxStack = 0;
    [Tooltip("사용 아이템(Use)일 경우의 쿨타임 (초)")]
    public float cooldown = 0f;

    // =================================================================
    //  자식들이 필요할 때만 가져다 덮어쓰기(override)합니다.
    // =================================================================

    /// <summary> 1. 스탯 재계산 시점 (패시브 스탯 증가용) </summary>
    public virtual void ApplyPassiveStat(PlayerStats stats, int stackCount) { }

    /// <summary> 2. 적에게 피해를 입힐 시점 (공격 적중 시 발동용) </summary>
    public virtual void OnHitEnemy(PlayerController player, GameObject target, float damage, int stackCount) { }

    /// <summary> 3. 적을 처치한 시점 (막타 시 발동용) </summary>
    public virtual void OnKillEnemy(PlayerController player, GameObject target, int stackCount) { }

    /// <summary> 4. 플레이어가 피해를 입을 시점 (피격 시 발동용) </summary>
    public virtual void OnTakeDamage(PlayerController player, float damageTaken, int stackCount) { }

    /// <summary> 5. 플레이어가 기본 공격을 실행할 시점 (천공 분쇄기 등) </summary>
    public virtual void OnBasicAttack(PlayerController player, int stackCount) { }

    /// <summary> 6. 액티브(사용) 아이템을 발동할 시점 </summary>
    /// <returns>발동 성공 여부 (true면 성공하여 쿨타임이 돌기 시작함)</returns>
    public virtual bool OnUse(PlayerController player) { return false; }
}
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Core Stats")]
    public float CurrentHealth => MaxHealth.Value; // 현재 체력은 최대 체력과 동일하게 설정 (추후 체력 감소 로직 추가 가능)
    public CharacterStat MaxHealth;
    public CharacterStat MoveSpeed;
    public CharacterStat Damage;
    public CharacterStat AttackSpeed;
    public CharacterStat CritChance;  // 0.0 ~ 1.0 (0% ~ 100%)
    public CharacterStat CritDamage;  // 1.5 = 150%, 2.0 = 200%

    [Header("Level & EXP")]
    public int CurrentLevel = 1;
    public int MaxLevel = 99; 
    public float CurrentExp = 0f;

    private void Awake()
    {
        MaxHealth = new CharacterStat(100f);
        MoveSpeed = new CharacterStat(5f);
        Damage = new CharacterStat(10f); 
        AttackSpeed = new CharacterStat(1f); 
        CritChance = new CharacterStat(0.01f); 
        CritDamage = new CharacterStat(2.0f); 
    }
    private void OnEnable()
    {
        EventBus.Subscribe<ItemPickedUpEvent>(OnItemPickedUp);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ItemPickedUpEvent>(OnItemPickedUp);
    }

    // 크리티컬 체크 도우미 함수 
    public bool RollCriticalHit()
    {
        return Random.value <= CritChance.Value;
    }

    public float GetRequiredExp(int level)
    {
        return 100f * Mathf.Pow(1.15f, level - 1);
    }

    // 경험치 획득 함수
    public void AddExp(float exp)
    {
        if (CurrentLevel >= MaxLevel) return;

        CurrentExp += exp;
        Debug.Log($"경험치 획득! (+{exp}) 현재 EXP: {CurrentExp} / {GetRequiredExp(CurrentLevel)}");

        // 경험치가 요구량을 넘으면 레벨 업 (여러 번 연속 렙업도 처리)
        while (CurrentExp >= GetRequiredExp(CurrentLevel) && CurrentLevel < MaxLevel)
        {
            CurrentExp -= GetRequiredExp(CurrentLevel);
            LevelUp();
        }
    }
    private void OnItemPickedUp(ItemPickedUpEvent e)
    {
        Debug.Log($"<color=green>[아이템 획득]</color> {e.ItemName} 적용 완료!");

        switch (e.TargetStat)
        {
            case EStatType.MoveSpeed: MoveSpeed.AddModifier(e.Modifier); break;
            case EStatType.Damage: Damage.AddModifier(e.Modifier); break;
            case EStatType.AttackSpeed: AttackSpeed.AddModifier(e.Modifier); break;
            case EStatType.MaxHealth: MaxHealth.AddModifier(e.Modifier); break;
            case EStatType.CritChance: CritChance.AddModifier(e.Modifier); break;
            case EStatType.CritDamage: CritDamage.AddModifier(e.Modifier); break;

            default: Debug.LogWarning($"알 수 없는 스탯 타입: {e.TargetStat}"); break;
        }
    }

    private void LevelUp()
    {
        CurrentLevel++;
        MaxHealth.BaseValue += 20f;
        Damage.BaseValue += 2f;

        // 체력 스탯이 변했으니 UI 업데이트 발행
        EventBus.Publish(new PlayerDamagedEvent
        {
            Amount = 0,
            CurrentHp = CurrentHealth,
            MaxHp = MaxHealth.Value
        });
    }
}
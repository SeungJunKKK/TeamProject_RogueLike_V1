using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Character Data")]
    public SurvivorData CharacterData;

    [Header("Core Stats")]
    [HideInInspector]public float CurrentHealth { get; set; } 
    [HideInInspector]public CharacterStat MaxHealth;
    [HideInInspector]public CharacterStat HealthRegen;
    [HideInInspector]public CharacterStat MoveSpeed;
    [HideInInspector]public CharacterStat Damage;
    [HideInInspector]public CharacterStat AttackSpeed;
    [HideInInspector]public CharacterStat CritChance;  // 0.0 ~ 1.0 (0% ~ 100%)
    [HideInInspector]public CharacterStat CritDamage;  // 1.5 = 150%, 2.0 = 200%
    [HideInInspector] public CharacterStat Armor;       // 방어력 (피해 감소율 계산에 사용)
    [HideInInspector] public CharacterStat CooldownReduction;   //쿨타임 감소 (Rare 아이템 적용) 


    [Header("Level & EXP")]
    public int CurrentLevel = 1;
    public int MaxLevel = 99; 
    public float CurrentExp = 0f;

    private void Awake()
    {
        // 데이터가 안 들어와 있으면 에러 방지용으로 기본값 세팅 (또는 에러 로그)
        if (CharacterData == null)
        {
            Debug.LogError("[PlayerStats] CharacterData가 할당되지 않았습니다!");
            return;
        }

        MaxHealth = new CharacterStat(CharacterData.BaseMaxHealth);
        HealthRegen = new CharacterStat(CharacterData.BaseHealthRegen);
        MoveSpeed = new CharacterStat(CharacterData.BaseMoveSpeed);
        Damage = new CharacterStat(CharacterData.BaseDamage);
        Armor = new CharacterStat(CharacterData.BaseArmor);
        AttackSpeed = new CharacterStat(CharacterData.BaseAttackSpeed);
        CritChance = new CharacterStat(CharacterData.BaseCritChance);
        CritDamage = new CharacterStat(CharacterData.BaseCritDamage);
        CooldownReduction = new CharacterStat(0f);
        CurrentHealth = MaxHealth.Value;
    }

    private void Update()
    {
        if (CurrentHealth > 0 && CurrentHealth < MaxHealth.Value)
        {
            CurrentHealth += HealthRegen.Value * Time.deltaTime;
            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth.Value);
        }
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
    public void IncreaseMaxHealth(float amount)
    {
        MaxHealth.BaseValue += amount;

        EventBus.Publish(new PlayerDamagedEvent
        {
            Amount = 0,
            CurrentHp = CurrentHealth,
            MaxHp = MaxHealth.Value
        });

        Debug.Log($"[PlayerStats] 최대 체력이 {amount}만큼 영구 증가했습니다!");
    }


    private void LevelUp()
    {
        CurrentLevel++;

        MaxHealth.BaseValue += CharacterData.HealthPerLevel;
        HealthRegen.BaseValue += CharacterData.HealthRegenPerLevel;
        Damage.BaseValue += CharacterData.DamagePerLevel;
        Armor.BaseValue += CharacterData.ArmorPerLevel;

        CurrentHealth += CharacterData.HealthPerLevel;

        EventBus.Publish(new PlayerDamagedEvent
        {
            Amount = 0,
            CurrentHp = CurrentHealth,
            MaxHp = MaxHealth.Value
        });

        Debug.Log($"[PlayerStats] 레벨업! 현재 레벨: {CurrentLevel}");
    }
}
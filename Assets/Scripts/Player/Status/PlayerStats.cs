using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Core Stats")]
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

    private void LevelUp()
    {
        CurrentLevel++;
        Debug.Log($"<color=cyan>레벨 업! 현재 레벨: {CurrentLevel}</color>");

       //추후 로그라이크 특성에 따라 능력 뽑기 추가 가능 
        MaxHealth.BaseValue += 20f;  
        Damage.BaseValue += 2f;      

    }
}
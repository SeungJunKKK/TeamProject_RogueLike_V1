public enum StatModType
{
    Flat = 100,         // 합연산 (공격력 +10)
    PercentAdd = 200,   // 퍼센트 합연산 (공격력 +10%, +20% -> 총 +30% 적용)
    PercentMult = 300   // 퍼센트 곱연산 (최종 공격력에 x1.5배)
}

public class StatModifier
{
    public readonly float Value;
    public readonly StatModType Type;
    public readonly object Source; 
  
    public StatModifier(float value, StatModType type, object source = null)
    {
        Value = value;
        Type = type;
        Source = source;
    }
}
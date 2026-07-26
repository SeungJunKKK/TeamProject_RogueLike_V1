using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterStat
{
    public float BaseValue; // 캐릭터의 순수 기본 능력치 

    
    protected bool m_IsDirty = true;
    protected float m_LastBaseValue = float.MinValue;
    protected float m_Value; 

    private readonly List<StatModifier> m_StatModifiers;

    public CharacterStat()
    {
        m_StatModifiers = new List<StatModifier>();
    }

    public CharacterStat(float baseValue) : this()
    {
        BaseValue = baseValue;
    }

    // 프로퍼티
    public float Value
    {
        get
        {
            if (m_IsDirty || BaseValue != m_LastBaseValue)
            {
                m_LastBaseValue = BaseValue;
                m_Value = CalculateFinalValue();
                m_IsDirty = false;
            }
            return m_Value;
        }
    }

    // 아이템 효과 추가
    public void AddModifier(StatModifier mod)
    {
        m_IsDirty = true;
        m_StatModifiers.Add(mod);

        // enum의 번호순(Flat -> PercentAdd -> PercentMult)
        m_StatModifiers.Sort(CompareModifierOrder);
    }

    // 특정 아이템 효과 제거 (아이템 버릴 때 사용)
    public bool RemoveModifier(StatModifier mod)
    {
        if (m_StatModifiers.Remove(mod))
        {
            m_IsDirty = true;
            return true;
        }
        return false;
    }

    public bool RemoveAllModifiersFromSource(object source)
    {
        bool didRemove = false;
        for (int i = m_StatModifiers.Count - 1; i >= 0; i--)
        {
            if (m_StatModifiers[i].Source == source)
            {
                m_IsDirty = true;
                didRemove = true;
                m_StatModifiers.RemoveAt(i);
            }
        }
        return didRemove;
    }

   
    protected virtual int CompareModifierOrder(StatModifier a, StatModifier b)
    {
        if (a.Type < b.Type)
        {
            return -1;
        }
        else if (a.Type > b.Type)
        {
            return 1;
        }

        return 0;
    }

    // 최종 데미지 계산
    protected virtual float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0;

        for (int i = 0; i < m_StatModifiers.Count; i++)
        {
            StatModifier mod = m_StatModifiers[i];

            if (mod.Type == StatModType.Flat)
            {
                finalValue += mod.Value; // 합연산 
            }
            else if (mod.Type == StatModType.PercentAdd)
            {
                sumPercentAdd += mod.Value; // 퍼센트끼리 

                // 다음 조절자가 PercentAdd가 아니거나 배열의 끝이면 곱셈
                if (i + 1 >= m_StatModifiers.Count || m_StatModifiers[i + 1].Type != StatModType.PercentAdd)
                {
                    finalValue *= (1.0f + sumPercentAdd);
                    sumPercentAdd = 0;
                }
            }
            else if (mod.Type == StatModType.PercentMult)
            {
                finalValue *= (1.0f + mod.Value); 
            }
        }
        
        return (float)Math.Round(finalValue, 4);
    }
}
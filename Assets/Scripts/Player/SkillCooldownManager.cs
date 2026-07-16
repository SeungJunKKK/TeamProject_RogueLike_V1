using System;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Primary_Z,
    Secondary_X,
    Utility_C,
    Ultimate_V
}

public class SkillCooldownManager : MonoBehaviour
{
    [Serializable]
    public class SkillData
    {
        public SkillType Type;
        public float MaxCooldown;
        [HideInInspector] public float CurrentCooldown;
    }

    [Header("Skill Cooldown Settings")]
    public List<SkillData> Skills = new List<SkillData>();
    //UI 직접 구독 
    public event Action<SkillType, float, float> OnSkillCooldownChanged;

    private void Start()
    {
        if (Skills.Count == 0)
        {
            Skills.Add(new SkillData { Type = SkillType.Primary_Z, MaxCooldown = 0.5f });
            Skills.Add(new SkillData { Type = SkillType.Secondary_X, MaxCooldown = 3.0f });
            Skills.Add(new SkillData { Type = SkillType.Utility_C, MaxCooldown = 1.0f });
            Skills.Add(new SkillData { Type = SkillType.Ultimate_V, MaxCooldown = 30.0f });
        }
    }

    private void Update()
    {
        foreach (var skill in Skills)
        {
            if (skill.CurrentCooldown > 0f)
            {
                skill.CurrentCooldown -= Time.deltaTime;

                if (skill.CurrentCooldown <= 0f)
                {
                    skill.CurrentCooldown = 0f;
                }
                OnSkillCooldownChanged?.Invoke(skill.Type, skill.CurrentCooldown, skill.MaxCooldown);
            }
        }
    }

    public bool IsSkillReady(SkillType type)
    {
        var skill = Skills.Find(s => s.Type == type);
        return skill != null && skill.CurrentCooldown <= 0f;
    }

    public void UseSkill(SkillType type)
    {
        var skill = Skills.Find(s => s.Type == type);
        if (skill != null && skill.CurrentCooldown <= 0f)
        {
            skill.CurrentCooldown = skill.MaxCooldown;
            OnSkillCooldownChanged?.Invoke(skill.Type, skill.CurrentCooldown, skill.MaxCooldown);
        }
    }
}
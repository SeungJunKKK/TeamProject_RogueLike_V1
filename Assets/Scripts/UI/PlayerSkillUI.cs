using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillUI : MonoBehaviour
{
    [SerializeField] private List<SkillSlotUI> skillSlots;
    private PlayerController player;

    private void Start()
    {
        InitPlayerRef();
    }

    private void Update()
    {
        if (player == null || player.CooldownManager == null)
        {
            InitPlayerRef();
            return;
        }

        foreach (var slot in skillSlots)
        {
            if (slot == null) continue;

            // SkillCooldownManager의 Skills 리스트에서 해당 타입 스킬 데이터 탐색
            var skillData = player.CooldownManager.Skills.Find(s => s.Type == slot.SkillType);
            if (skillData != null)
            {
                slot.UpdateCooldown(skillData.CurrentCooldown, skillData.MaxCooldown);
            }
        }
    }

    private void InitPlayerRef()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }
    }
}
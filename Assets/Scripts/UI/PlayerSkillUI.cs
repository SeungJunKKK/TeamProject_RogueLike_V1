using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillUI : MonoBehaviour
{
    [SerializeField] private List<SkillSlotUI> skillSlots;
    private PlayerController player;
    private bool isUIInitialized = false;

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

        if (player.CooldownManager == null) return;

        // 1. 플레이어 참조를 찾았고, SurvivorData가 유효할 때 1회 초기화
        if (!isUIInitialized && player.SurvivorData != null)
        {
            InitSkillUI();
        }

        // 2. 매 프레임 쿨타임 업데이트
        foreach (var slot in skillSlots)
        {
            if (slot == null) continue;

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
            player = FindAnyObjectByType<PlayerController>();
        }
    }

    private void InitSkillUI()
    {
        if (player == null) return;

        // PlayerController에 연결된 SurvivorData 또는 DataSO를 참조합니다.
        // (필요시 player.Data 또는 player.SurvivorData 명칭에 맞춰 수정하세요)
        SurvivorData data = player.SurvivorData;

        if (data == null || skillSlots == null) return;

        // SkillSlotUI 개수에 맞춰 SkillInfo 할당
        if (skillSlots.Count > 0) skillSlots[0]?.SetSkill(data.PrimarySkill);
        if (skillSlots.Count > 1) skillSlots[1]?.SetSkill(data.SecondarySkill);
        if (skillSlots.Count > 2) skillSlots[2]?.SetSkill(data.UtilitySkill);
        if (skillSlots.Count > 3) skillSlots[3]?.SetSkill(data.UltimateSkill);
        if (skillSlots.Count > 4) skillSlots[4]?.SetSkill(data.StrengthenedUltimateSkill);

        isUIInitialized = true;
    }
}
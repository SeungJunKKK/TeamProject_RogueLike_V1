using UnityEngine;

[CreateAssetMenu(fileName = "New Survivor", menuName = "RoR/Survivor Data")]
public class SurvivorData : ScriptableObject
{
    [Header("Basic Info")]
    public string SurvivorName;
    [TextArea] public string Description;

    [Header("In-Game Prefab & UI")]
    public GameObject PlayerPrefab; // 실제 게임에서 스폰될  프리팹
    public Sprite SelectSprite;     // 선택창 포드 안에 보여질 도트 이미지
    public RuntimeAnimatorController SelectAnimController;

    [Header("Skills (Z, X, C, V)")]
    public SkillInfo PrimarySkill;
    public SkillInfo SecondarySkill;
    public SkillInfo UtilitySkill;
    public SkillInfo UltimateSkill;

    [Header("Strengthened / Alternate Skills")]
    [Tooltip("에인션트 셉터 획득 시 강화되는 궁극기나, 교체 가능한 대체 스킬을 넣습니다.")]
    public SkillInfo StrengthenedUltimateSkill;

    [Header("Base Stats (기본 능력치)")]
    public float BaseMaxHealth = 110f;
    public float BaseHealthRegen = 0.6f;
    public float BaseDamage = 12f;
    public float BaseArmor = 0f;
    public float BaseMoveSpeed = 1.3f;
    public float BaseAttackSpeed = 2f;
    public float BaseCritChance = 0.1f;
    public float BaseCritDamage = 2.0f;

    [Header("Level Up Stats (레벨당 상승치)")]
    public float HealthPerLevel = 32f;
    public float HealthRegenPerLevel = 0.12f;
    public float DamagePerLevel = 3f;
    public float ArmorPerLevel = 2f;


}

[System.Serializable]
public struct SkillInfo
{
    public string SkillName;
    [TextArea] public string SkillDescription;
    public Sprite SkillIcon;
}
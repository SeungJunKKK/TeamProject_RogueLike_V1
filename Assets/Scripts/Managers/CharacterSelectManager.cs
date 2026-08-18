using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterSelectManager : MonoBehaviour
{
    public static CharacterSelectManager Instance;

    [Header("Left Panel UI")]
    public GameObject LeftInfoPanel;
    public TextMeshProUGUI SurvivorNameText;
    public TextMeshProUGUI DescriptionText;


    [Header("Skill UI (0:Z, 1:X, 2:C, 3:V)")]
    public Image[] SkillIcons;
    public TextMeshProUGUI[] SkillNames;
    public TextMeshProUGUI[] SkillDescs;
    public SkillSlotUI[] SkillSlots;

    [Header("Difficulty UI")]
    public Image[] DifficultyImages;           // 0: 이슬비, 1: 폭풍우, 2: 몬순 버튼의 Image 컴포넌트
    public Sprite[] NormalDifficultySprites;   // 비활성화 회색 스프라이트
    public Sprite[] SelectedDifficultySprites; // 활성화 컬러 스프라이트 

    private EGameDifficulty m_SelectedDifficulty = EGameDifficulty.Drizzle; 


    [Header("Game Flow")]
    public Button StartButton;
    private SurvivorData m_SelectedSurvivor;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (LeftInfoPanel != null)
        {
            LeftInfoPanel.SetActive(false);
        }
        SetDifficultyUI(0);
    }

    public void OnDifficultyButtonClicked(int difficultyIndex)
    {
        m_SelectedDifficulty = (EGameDifficulty)difficultyIndex;
        SetDifficultyUI(difficultyIndex);
    }

    private void SetDifficultyUI(int selectedIndex)
    {
        for (int i = 0; i < DifficultyImages.Length; i++)
        {
            if (i == selectedIndex)
            {
                DifficultyImages[i].sprite = SelectedDifficultySprites[i];
            }
            else
            {
                DifficultyImages[i].sprite = NormalDifficultySprites[i];
            }
        }
    }

    public void OnSurvivorSelected(SurvivorData data)
    {
        if (LeftInfoPanel != null)
        {
            LeftInfoPanel.SetActive(true);
        }

        m_SelectedSurvivor = data;

        if (SurvivorNameText != null) SurvivorNameText.text = data.SurvivorName;
        if (DescriptionText != null) DescriptionText.text = data.Description;

        UpdateSkillUI(0, data.PrimarySkill);
        UpdateSkillUI(1, data.SecondarySkill);
        UpdateSkillUI(2, data.UtilitySkill);
        UpdateSkillUI(3, data.UltimateSkill);
        UpdateSkillUI(4, data.StrengthenedUltimateSkill);

        StartButton.interactable = true;
    }

    private void UpdateSkillUI(int index, SkillInfo info)
    {
        if (SkillIcons != null && index < SkillIcons.Length && SkillIcons[index] != null)
        {
            SkillIcons[index].sprite = info.SkillIcon;
            SkillIcons[index].enabled = (info.SkillIcon != null);
        }

        if (SkillNames != null && index < SkillNames.Length && SkillNames[index] != null)
        {
            SkillNames[index].text = info.SkillName;
        }

        if (SkillDescs != null && index < SkillDescs.Length && SkillDescs[index] != null)
        {
            SkillDescs[index].text = info.SkillDescription;
        }

        if (SkillSlots != null && index < SkillSlots.Length && SkillSlots[index] != null)
        {
            SkillSlots[index].SetSkill(info);
        }
    }

    public void OnStartButtonClicked()
    {
        if (m_SelectedSurvivor != null)
        {
            if (DifficultyManager.Instance != null)
            {
                DifficultyManager.Instance.SetGameDifficulty(m_SelectedDifficulty);
            }

            GameManager.Instance.SetSelectedPlayer(m_SelectedSurvivor.PlayerPrefab);
            SceneLoader.Instance.LoadScene("Stage1_Scene", true, "Shared"); // 실제 게임 씬 이름으로 변경
        }
    }
}
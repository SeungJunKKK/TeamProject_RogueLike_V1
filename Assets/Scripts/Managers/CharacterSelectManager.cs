using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

        StartButton.interactable = true;
    }

    private void UpdateSkillUI(int index, SkillInfo info)
    {
        SkillIcons[index].sprite = info.SkillIcon;
        SkillNames[index].text = info.SkillName;
        SkillDescs[index].text = info.SkillDescription;
    }

    public void OnStartButtonClicked()
    {
        if (m_SelectedSurvivor != null)
        {
            // TODO: 선택된 프리팹 정보를 GameManager에 넘겨주어야 함
            // GameManager.Instance.SetPlayerPrefab(m_SelectedSurvivor.PlayerPrefab);

            SceneManager.LoadScene("Stage1_Scene"); // 실제 게임 씬 이름으로 변경
        }
    }
}
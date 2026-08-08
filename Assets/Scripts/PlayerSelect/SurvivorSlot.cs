using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SurvivorSlot : MonoBehaviour
{
    [Tooltip("여기에 SurvivorData를 넣으면 해당 캐릭터 슬롯이 됩니다. 비워두면 선택 불가 빈칸이 됩니다.")]
    public SurvivorData Data;
    [Header("UI Components")]
    public Image CharacterImage;
    [Tooltip("캐릭터 데이터가 없을 때 띄워줄 물음표/잠김 이미지입니다.")]
    public Sprite LockedSprite;

    [Header("Animation")]
    public Animator SlotAnimator; // 슬롯 애니메이션을 담당할 컴포넌트

    private Button m_Button;

    private void Awake()
    {
        m_Button = GetComponent<Button>();
        m_Button.onClick.AddListener(OnSlotClicked);

        if (Data != null)
        {
            CharacterImage.sprite = Data.SelectSprite;
            m_Button.interactable = true;

            if (Data.SelectAnimController != null && SlotAnimator != null)
            {
                SlotAnimator.runtimeAnimatorController = Data.SelectAnimController;
                SlotAnimator.enabled = false;
            }
        }
        else
        {
            CharacterImage.sprite = LockedSprite;
            m_Button.interactable = false;
        }
    }

    private void OnSlotClicked()
    {
        if (Data != null)
        {
            CharacterSelectManager.Instance.OnSurvivorSelected(Data);

            if (SlotAnimator != null)
            {
                SlotAnimator.enabled = true;
                SlotAnimator.SetTrigger("Select");
            }
        }
    }
}
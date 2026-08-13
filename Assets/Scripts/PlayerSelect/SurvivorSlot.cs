using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Button))]
public class SurvivorSlot : MonoBehaviour
{
    public SurvivorData Data;
    [Header("UI Components")]
    public Image CharacterImage;
    public Sprite LockedSprite;

    [Header("Animation")]
    public Animator SlotAnimator;

    private Button m_Button;
    private static List<SurvivorSlot> s_AllSlots = new List<SurvivorSlot>();

    private void Awake()
    {
        m_Button = GetComponent<Button>();
        m_Button.onClick.AddListener(OnSlotClicked);
        s_AllSlots.Add(this);
    }

    private void OnDestroy()
    {
        s_AllSlots.Remove(this);
    }

    private void Start()
    {
        if (Data != null)
        {
            ResetToDefault();
            m_Button.interactable = true;
        }
        else
        {
            CharacterImage.sprite = LockedSprite;
            m_Button.interactable = false;
            if (SlotAnimator != null) SlotAnimator.enabled = false;
        }
    }

    private void OnSlotClicked()
    {
        if (Data != null)
        {
            foreach (var slot in s_AllSlots)
            {
                if (slot != this)
                {
                    slot.ResetToDefault();
                }
            }

            CharacterSelectManager.Instance.OnSurvivorSelected(Data);

            if (Data.SelectAnimController != null && SlotAnimator != null)
            {
                SlotAnimator.runtimeAnimatorController = Data.SelectAnimController;
                SlotAnimator.enabled = true; 
                SlotAnimator.SetTrigger("Select");
            }
        }
    }

    public void ResetToDefault()
    {
        if (SlotAnimator != null)
        {
            SlotAnimator.Rebind();
            SlotAnimator.Update(0f);
            SlotAnimator.enabled = false;
        }

        if (Data != null && CharacterImage != null)
        {
            CharacterImage.gameObject.SetActive(true);

            if (Data.SelectSprite != null)
            {
                CharacterImage.sprite = Data.SelectSprite;
            }
            else
            {
                Debug.LogWarning($"<color=yellow>[경고] {Data.SurvivorName}의 SelectSprite가 비어있습니다! 인스펙터를 확인하세요.</color>");
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TMP_Text stackCountText;

    public void SetItem(ItemData itemData, int stackCount)
    {
        // 아이콘 설정
        if (itemIconImage != null && itemData.itemIcon != null)
        {
            itemIconImage.sprite = itemData.itemIcon;
            itemIconImage.gameObject.SetActive(true);
        }

        // 중첩 수 설정
        if (stackCountText != null)
        {
            if (stackCount > 1)
            {
                stackCountText.text = $"x{stackCount}";
                stackCountText.gameObject.SetActive(true);
            }
            else
            {
                stackCountText.gameObject.SetActive(false);
            }
        }
    }
}
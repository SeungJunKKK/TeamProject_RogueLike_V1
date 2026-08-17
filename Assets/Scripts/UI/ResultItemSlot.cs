using UnityEngine;
using UnityEngine.UI;

public class ResultItemSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image m_IconImage; 

    public void Setup(ItemData itemData)
    {
        if (itemData == null) return;

        if (itemData.itemIcon != null)
        {
            m_IconImage.sprite = itemData.itemIcon;
            m_IconImage.gameObject.SetActive(true);
        }
        else
        {
            m_IconImage.gameObject.SetActive(false);
        }

        Debug.Log($"<color=green>[아이콘 슬롯 생성 완료] {itemData.itemName}</color>[cite: 9]");
    }
}
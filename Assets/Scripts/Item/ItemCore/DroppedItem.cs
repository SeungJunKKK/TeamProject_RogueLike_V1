using UnityEngine;

public class DroppedItem : MonoBehaviour, IInteractable
{
    [SerializeField] private SpriteRenderer m_SpriteRenderer;
    [SerializeField] private AudioClip m_PickupSound;
    private ItemData m_ItemData;

    public void Initialize(ItemData itemData)
    {
        m_ItemData = itemData;
        if (m_SpriteRenderer != null && itemData != null)
        {
            m_SpriteRenderer.sprite = itemData.itemIcon;
        }
    }

    public void Interact(GameObject interactor)
    {
        // 아이템 데이터 자체 검증
        if (m_ItemData == null)
        {
            Debug.LogWarning("[DroppedItem] ItemData가 설정되지 않은 드랍 아이템입니다.");
            return;
        }

        if (interactor != null)
        {
            // 부모/자식 오브젝트 포함하여 PlayerInventory 검색
            PlayerInventory inventory = interactor.GetComponentInChildren<PlayerInventory>();

            if (inventory != null)
            {
                // 인벤토리에 아이템 추가
                inventory.AddItem(m_ItemData);

                // 획득 SFX 재생
                PlayPickupSound();

                // 텍스트 팝업 이벤트 발행
                Vector3 textPosition = transform.position + Vector3.up * 0.8f;

                EventBus.Publish(new ItemPickupTextPopupEvent
                {
                    WorldPosition = textPosition,
                    ItemName = m_ItemData.itemName,
                    ItemDescription = m_ItemData.description,
                    ItemColor = m_ItemData.tier.GetColor()
                });

                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("[DroppedItem] 플레이어 오브젝트에서 PlayerInventory 컴포넌트를 찾을 수 없습니다!");
            }
        }
    }

    private void PlayPickupSound()
    {
        if (m_PickupSound != null)
        {
            SoundManager.Instance.PlaySFX(m_PickupSound);
        }
    }

    public string GetPromptText()
    {
        if (m_ItemData == null) return "";
        string hex = ColorUtility.ToHtmlStringRGB(m_ItemData.tier.GetColor());
        return $"<color=#{hex}>[{m_ItemData.itemName}]</color> 획득 [G]";
    }
}
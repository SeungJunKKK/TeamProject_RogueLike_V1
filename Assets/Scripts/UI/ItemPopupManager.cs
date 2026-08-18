using UnityEngine;

public class ItemPopupManager : MonoBehaviour
{
    [SerializeField] private ItemTextPopup m_PopupPrefab;

    private void OnEnable()
    {
        EventBus.Subscribe<ItemPickupTextPopupEvent>(OnItemPickupPopup);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ItemPickupTextPopupEvent>(OnItemPickupPopup);
    }

    private void OnItemPickupPopup(ItemPickupTextPopupEvent evt)
    {
        if (m_PopupPrefab == null) return;

        ItemTextPopup popup = Instantiate(m_PopupPrefab, evt.WorldPosition, Quaternion.identity);
        popup.Setup(evt.ItemName, evt.ItemDescription, evt.ItemColor);
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float m_Range = 1.0f;
    [SerializeField] private LayerMask m_InteractableLayer;   // 인스펙터에서 Interactable(15) 지정

    private IInteractable m_Current;

    private void Update()
    {
        DetectInteractable();
        HandleInteractionInput();
    }

    private void DetectInteractable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, m_Range, m_InteractableLayer);

        IInteractable found = null;

        // 범위 내 첫 번째 감지 대상을 선택
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IInteractable interactable))
            {
                found = interactable;
                break;
            }
        }

        if (found == m_Current)
        {
            return;
        }

        m_Current = found;

        if (found != null)
        {
            EventBus.Publish(new InteractableInRangeEvent
            {
                WorldPosition = transform.position + Vector3.up * 0.5f,
                PromptText = found.GetPromptText()
            });
        }
        else
        {
            EventBus.Publish(new InteractableOutOfRangeEvent());
        }
    }

    private void HandleInteractionInput()
    {
        if (Keyboard.current == null) return;

        bool isUpPressed = Keyboard.current.upArrowKey.wasPressedThisFrame;
        bool isGPressed = Keyboard.current.gKey.wasPressedThisFrame;

        // 키 입력이 없으면 탐색하지 않음
        if (!isUpPressed && !isGPressed) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, m_Range, m_InteractableLayer);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IInteractable interactable))
            {
                // Up 키 누름: 상자(BaseChest) 또는 텔레포터(Teleporter) 상호작용
                if (isUpPressed && (interactable is BaseChest || interactable is Teleporter))
                {
                    interactable.Interact(gameObject);
                    break;
                }
                // G 키 누름: 바닥 아이템(DroppedItem) 상호작용
                else if (isGPressed && interactable is DroppedItem)
                {
                    interactable.Interact(gameObject);
                    break;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, m_Range);
    }
}

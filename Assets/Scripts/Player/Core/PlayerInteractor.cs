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
        // 범위 내 모든 Interactable 레이어 콜라이더 감지
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, m_Range, m_InteractableLayer);

        IInteractable found = null;

        // 우선순위: 바닥 아이템(DroppedItem)이 있으면 아이템을, 없으면 다른 상호작용 대상(상자 등)을 선택
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IInteractable interactable))
            {
                if (interactable is DroppedItem)
                {
                    found = interactable;
                    break; // 아이템을 최우선으로 감지
                }
                else if (found == null)
                {
                    found = interactable;
                }
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

        // 범위 내 감지된 모든 대상을 확인하여 눌린 키에 맞는 상호작용 실행
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, m_Range, m_InteractableLayer);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IInteractable interactable))
            {
                // 상자: 방향키 위(Up) 입력 시 실행
                if (interactable is BaseChest && Keyboard.current.upArrowKey.wasPressedThisFrame)
                {
                    interactable.Interact(gameObject);
                    break;
                }
                // 바닥 아이템: G 키 입력 시 실행
                else if (interactable is DroppedItem && Keyboard.current.gKey.wasPressedThisFrame)
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

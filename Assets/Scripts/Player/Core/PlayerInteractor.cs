using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float m_Range = .3f;
    [SerializeField] private LayerMask m_InteractableLayer;   // 인스펙터에서 Interactable(15) 지정

    private IInteractable m_Current;

    private void Update()
    {
        DetectInteractable();

        if (Keyboard.current != null && Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            m_Current?.Interact(gameObject);
        }
    }

    private void DetectInteractable()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, m_Range, m_InteractableLayer);

        IInteractable found = null;

        if (hit != null)
        {
            hit.TryGetComponent(out found);
        }

        if (found == m_Current)
        {
            return;
        }

        m_Current = found;

        if (found != null)
        {
            EventBus.Publish(new InteractableInRangeEvent { WorldPosition = hit.transform.position, PromptText = found.GetPromptText() });
        }
        else
        {
            EventBus.Publish(new InteractableOutOfRangeEvent());
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, m_Range);
    }
}

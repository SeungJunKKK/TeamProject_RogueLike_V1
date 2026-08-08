using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float m_Range = 2f;
    [SerializeField] private LayerMask m_InteractableLayer;   // 인스펙터에서 Interactable(15) 지정

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }
        // 위 화살표 키를 눌렀을 때 상호작용 시도
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, m_Range, m_InteractableLayer);
        if (hit == null)
        {
            return;
        }

        if (hit.TryGetComponent(out IInteractable interactable))
        {
            interactable.Interact(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, m_Range);
    }
}

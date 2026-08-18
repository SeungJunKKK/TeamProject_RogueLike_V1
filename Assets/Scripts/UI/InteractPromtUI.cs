using TMPro;
using UnityEngine;

public class InteractPromtUI : MonoBehaviour
{
    [SerializeField] private GameObject m_PromtRoot;
    [SerializeField] private TextMeshProUGUI m_Text;
    [SerializeField] private Vector3 m_WorldOffset = new Vector3(0f, 1f, 0f);

    private Vector3 m_TargetWorldPos;
    private bool m_IsShowing;

    private void OnEnable()
    {
        EventBus.Subscribe<InteractableInRangeEvent>(OnInRange);
        EventBus.Subscribe<InteractableOutOfRangeEvent>(OnOutOfRange);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<InteractableInRangeEvent>(OnInRange);
        EventBus.Unsubscribe<InteractableOutOfRangeEvent>(OnOutOfRange);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<InteractableInRangeEvent>(OnInRange);
        EventBus.Unsubscribe<InteractableOutOfRangeEvent>(OnOutOfRange);
    }

    private void OnInRange(InteractableInRangeEvent e)
    {
        if (string.IsNullOrEmpty(e.PromptText))   
        {
            if (m_PromtRoot != null) m_PromtRoot.SetActive(false);
            m_IsShowing = false;
            return;
        }

        m_Text.text = e.PromptText;
        m_TargetWorldPos = e.WorldPosition;   
        m_IsShowing = true;

        if (m_PromtRoot != null)
        {
            m_PromtRoot.SetActive(true);
        }
    }

    private void OnOutOfRange(InteractableOutOfRangeEvent e)
    {
        m_IsShowing = false;
        if (m_PromtRoot != null)
        {
            m_PromtRoot.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (!m_IsShowing || m_PromtRoot == null)
        {
            return;
        }

        Vector3 screenPos = Camera.main.WorldToScreenPoint(m_TargetWorldPos + m_WorldOffset);
        m_PromtRoot.transform.position = screenPos;
    }
}
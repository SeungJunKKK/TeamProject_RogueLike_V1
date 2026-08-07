using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [SerializeField] private int m_Price = 25;

    private Animator m_Animator;
    private bool m_IsOpened;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 상자를 연다. 골드가 충분하면 차감하고 열림 애니메이션 재생.
    /// </summary>
    public void Interact(GameObject interactor)
    {
        if (m_IsOpened)
        {
            return;
        }    

        if (!GameManager.Instance.TrySpendGold(m_Price))
        {
            Debug.Log($"[Chest] 골드 부족 (필요: {m_Price}, 보유: {GameManager.Instance.Gold})");
            return;
        }

        Open();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public void DebugOpen()
    {
        if (m_IsOpened)
        {
            return;
        }
        Open();
    }
#endif

    private void Open()
    {
        m_IsOpened = true;
        m_Animator.SetTrigger("Open");

        Debug.Log($"[Chest] 열림! 아이템 지급 예정");
        // TODO: 아이템 스폰 (P3 연결)
    }
}

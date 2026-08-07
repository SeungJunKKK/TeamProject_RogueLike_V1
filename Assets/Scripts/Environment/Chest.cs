using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [SerializeField] private int m_Price = 25;

    [Header("Item Drop Settings")]
    [Tooltip("이 상자에서 나올 수 있는 아이템 데이터들을 인스펙터에서 넣어주세요.")]
    [SerializeField] private ItemData[] m_PossibleItems;

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

        Open(interactor);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public void DebugOpen()
    {
        if (m_IsOpened)
        {
            return;
        }
        PlayerController player = FindAnyObjectByType<PlayerController>();
        Open(player != null ? player.gameObject : null);
    }
#endif

    private void Open(GameObject interactor)
    {
        m_IsOpened = true;
        m_Animator.SetTrigger("Open");

        // ==========================================
        // 아이템 랜덤 뽑기 및 지급 로직 -> Todo : 아이템 지급 로직 분리 필요, 랜덤 확률화 필요.
        // ==========================================
        if (m_PossibleItems != null && m_PossibleItems.Length > 0)
        {
            int randomIndex = Random.Range(0, m_PossibleItems.Length);
            ItemData drawnItem = m_PossibleItems[randomIndex];

            Debug.Log($"<color=orange>[Chest] 열림! {drawnItem.itemName} 획득 예정!</color>");

            if (interactor != null)
            {
                PlayerInventory inventory = interactor.GetComponent<PlayerInventory>();
                if (inventory != null)
                {
                    inventory.AddItem(drawnItem);
                }
            }
        }
        else
        {
            Debug.LogWarning("[Chest] 상자에 설정된 아이템 풀(Possible Items)이 비어있습니다!");
        }
    }
}
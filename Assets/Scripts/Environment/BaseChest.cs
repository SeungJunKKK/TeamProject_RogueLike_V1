using UnityEngine;

public abstract class BaseChest : MonoBehaviour, IInteractable
{
    [SerializeField] protected AudioClip m_OpenSound;

    [Header("Chest Tier")]
    [SerializeField] protected ItemTier m_Tier = ItemTier.Common;

    [Header("Item Drop Settings")]
    [Tooltip("이 상자에서 나올 수 있는 아이템 데이터들")]
    [SerializeField] protected ItemData[] m_PossibleItems;
    [SerializeField] protected DroppedItem m_DroppedItemPrefab;

    protected Animator m_Animator;

    protected virtual void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    public void Interact(GameObject interactor)
    {
        if (!CanOpen())
        {
            return;
        }

        int price = GetPrice();
        if (!GameManager.Instance.TrySpendGold(price))
        {
            Debug.Log($"[Chest] 골드 부족 (필요: {price}, 보유: {GameManager.Instance.Gold})");
            return;
        }

        OnOpened();
        Open(interactor);

        EventBus.Publish(new InteractableInRangeEvent
        {
            WorldPosition = transform.position,
            PromptText = GetPromptText()
        });
    }

    public string GetPromptText()
    {
        if (!CanOpen())
        {
            return "";
        }

        string hex = ColorUtility.ToHtmlStringRGB(m_Tier.GetColor());
        return $"<color=#{hex}>[{m_Tier}] Chest</color> {GetPrice()}G [Up]";
    }
    protected abstract bool CanOpen();
    protected abstract int GetPrice();
    protected abstract void OnOpened();

    protected virtual void PlayOpenSound()
    {
        if (m_OpenSound != null)
        {
            SoundManager.Instance.PlaySFX(m_OpenSound);
        }
    }

    protected virtual void Open(GameObject interactor)
    {
        m_Animator.SetTrigger("Open");
        PlayOpenSound();
        DropRandomItem();
    }

    private void DropRandomItem()
    {
        if (m_PossibleItems == null || m_PossibleItems.Length == 0) return;
        if (m_DroppedItemPrefab == null) return;

        int randomIndex = Random.Range(0, m_PossibleItems.Length);
        ItemData drawnItem = m_PossibleItems[randomIndex];

        // 상자 오른쪽 앞 바닥 위치 (Y축 오프셋을 낮춤)
        // 맵 구조에 따라 (0.8f, -0.2f) 숫자를 살짝 조절해 보세요.
        Vector3 dropPosition = transform.position + new Vector3(0.8f, -0.2f, 0f);

        DroppedItem droppedItem = Instantiate(m_DroppedItemPrefab, dropPosition, Quaternion.identity);
        droppedItem.Initialize(drawnItem);
    }

    private void GiveRandomItem(GameObject interactor)
    {
        if (m_PossibleItems == null || m_PossibleItems.Length == 0)
        {
            Debug.LogWarning("[Chest] 상자에 설정된 아이템 풀이 비어있습니다!");
            return;
        }

        int randomIndex = Random.Range(0, m_PossibleItems.Length);
        ItemData drawnItem = m_PossibleItems[randomIndex];
        Debug.Log($"<color=orange>[Chest] 열림! {drawnItem.itemName} 획득 예정!</color>");

        if (interactor != null && interactor.TryGetComponent(out PlayerInventory inventory))
        {
            inventory.AddItem(drawnItem);
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public void DebugOpen()
    {
        if (!CanOpen())
        {
            return;
        }
        OnOpened();
        PlayerController player = FindAnyObjectByType<PlayerController>();
        Open(player != null ? player.gameObject : null);
    }
#endif
}

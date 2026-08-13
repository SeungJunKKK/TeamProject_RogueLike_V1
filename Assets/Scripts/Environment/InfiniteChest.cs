using UnityEngine;

public class InfiniteChest : BaseChest
{
    [SerializeField] private int m_BasePrice = 25;
    [SerializeField] private float m_PriceMultiplier = 1.5f;   // 열 때마다 배수
    [SerializeField] private AudioClip[] m_OpenSounds;

    private int m_OpenCount;

    protected override bool CanOpen()
    {
        return !IsOpening();
    }

    private bool IsOpening()
    {
        AnimatorStateInfo state = m_Animator.GetCurrentAnimatorStateInfo(0);
        return state.IsName("InfiniteChestOpen") && state.normalizedTime < 1f;
    }
    protected override int GetPrice() => Mathf.RoundToInt(m_BasePrice * Mathf.Pow(m_PriceMultiplier, m_OpenCount));
    protected override void OnOpened() => m_OpenCount++;

    protected override void PlayOpenSound()
    {
        if (m_OpenSounds != null && m_OpenSounds.Length > 0)
        {
            SoundManager.Instance.PlaySFX(m_OpenSounds[Random.Range(0, m_OpenSounds.Length)]);
        }
    }
}

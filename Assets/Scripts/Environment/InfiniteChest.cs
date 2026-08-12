using UnityEngine;

public class InfiniteChest : BaseChest
{
    [SerializeField] private int m_BasePrice = 25;
    [SerializeField] private float m_PriceMultiplier = 1.5f;   // 열 때마다 배수

    private int m_OpenCount;

    protected override bool CanOpen() => true;
    protected override int GetPrice() => Mathf.RoundToInt(m_BasePrice * Mathf.Pow(m_PriceMultiplier, m_OpenCount));
    protected override void OnOpened() => m_OpenCount++; 
}

using UnityEngine;

public class Chest : BaseChest
{
    [SerializeField] private int m_Price = 25;

    private bool m_IsOpened;

    protected override bool CanOpen() => !m_IsOpened;
    protected override int GetPrice() => m_Price;
    protected override void OnOpened() => m_IsOpened = true;
}
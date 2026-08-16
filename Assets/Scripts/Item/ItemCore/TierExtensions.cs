using UnityEngine;

public static class TierExtensions
{
    public static Color GetColor(this ItemTier tier)
    {
        return tier switch
        {
            ItemTier.Common => Color.white,                    // 하양
            ItemTier.UnCommon => new Color(0.45f, 0.9f, 0.45f),  // 초록
            ItemTier.Rare => new Color(1f, 0.35f, 0.35f),    // 빨강
            ItemTier.Boss => new Color(1f, 0.85f, 0.1f),     // 노랑
            ItemTier.Use => new Color(1f, 0.6f, 0.15f),     // 주황
            _ => throw new System.ArgumentOutOfRangeException(nameof(tier), tier, null)
        };
    }
}
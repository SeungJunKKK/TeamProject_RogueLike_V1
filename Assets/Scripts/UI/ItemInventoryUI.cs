using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemInventoryUI : MonoBehaviour
{
    [Header("UI Component Settings")]
    [SerializeField] private Transform itemSlotContainer;
    [SerializeField] private GameObject itemSlotPrefab;

    [Header("Player Inventory Reference")]
    [SerializeField] private PlayerInventory playerInventory;

    private Dictionary<ItemData, ItemSlotUI> activeSlots = new Dictionary<ItemData, ItemSlotUI>();

    private void Start()
    {
        if (playerInventory == null)
            playerInventory = FindAnyObjectByType<PlayerInventory>();

        RefreshAllSlots();
    }

    private void Update()
    {
        RefreshAllSlots();
    }

    public void RefreshAllSlots()
    {
        if (playerInventory == null) return;

        foreach (var pair in playerInventory.passiveItems)
        {
            ItemData item = pair.Key;
            int count = pair.Value;

            // 이미 슬롯이 생성되어 있다면 숫자만 갱신
            if (activeSlots.TryGetValue(item, out ItemSlotUI existingSlot))
            {
                existingSlot.SetItem(item, count);
            }
            // 새로운 아이템이면 슬롯 프리팹을 동적으로 생성 후 추가
            else
            {
                GameObject newSlotObj = Instantiate(itemSlotPrefab, itemSlotContainer);
                ItemSlotUI newSlot = newSlotObj.GetComponent<ItemSlotUI>();

                if (newSlot != null)
                {
                    newSlot.SetItem(item, count);
                    activeSlots.Add(item, newSlot);
                }
            }
        }
    }
}
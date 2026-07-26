using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemLogManager : MonoBehaviour
{
    [Header("UI Slots Parent Containers")]
    [SerializeField] private Transform commonContainer;
    [SerializeField] private Transform uncommonContainer;
    [SerializeField] private Transform rareContainer;
    [SerializeField] private Transform useContainer;

    [Header("Item Slot Prefab Key or Reference")]
    [SerializeField] private GameObject itemSlotPrefab; // 슬롯 기본 프리팹

    [Header("Addressable Keys")]
    // Addressable Inspector에 지정한 Key 이름들
    [SerializeField] private string commonKey = "ItemLog_Common";
    [SerializeField] private string uncommonKey = "ItemLog_Uncommon";
    [SerializeField] private string rareKey = "ItemLog_Rare";
    [SerializeField] private string useKey = "ItemLog_Use";

    private bool _isLoaded = false;

    private void OnEnable()
    {
        // Item Log 팝업이 활성화될 때 에셋을 로드하여 생성
        if (!_isLoaded)
        {
            LoadCategorySprites(commonKey, commonContainer);
            LoadCategorySprites(uncommonKey, uncommonContainer);
            LoadCategorySprites(rareKey, rareContainer);
            LoadCategorySprites(useKey, useContainer);
            _isLoaded = true;
        }
    }

    private void LoadCategorySprites(string key, Transform container)
    {
        if (container == null || string.IsNullOrEmpty(key)) return;

        AddressableManager.Instance.LoadSpriteSheetAsync(key, (sprites) =>
        {
            if (sprites != null && sprites.Count > 0)
            {
                foreach (Sprite sprite in sprites)
                {
                    GameObject slot = Instantiate(itemSlotPrefab, container);
                    Image img = slot.GetComponent<Image>();
                    if (img == null) img = slot.GetComponentInChildren<Image>();

                    if (img != null)
                    {
                        img.sprite = sprite;
                        img.color = Color.white;
                    }
                }
            }
        });
    }

    //private void LoadAndGenerateCategory(string addressableKey, Transform container)
    //{
    //    if (container == null) return;

    //    // AddressableManager를 통해 Sprite 에셋 로드
    //    AddressableManager.Instance.LoadAssetAsync<Sprite>(addressableKey, (sprite) =>
    //    {
    //        if (sprite != null)
    //        {
    //            // 로드 성공 시 슬롯 생성 및 스프라이트 할당
    //            GameObject slot = Instantiate(itemSlotPrefab, container);
    //            Image img = slot.GetComponent<Image>();
    //            if (img != null)
    //            {
    //                img.sprite = sprite;
    //            }
    //        }
    //        else
    //        {
    //            Debug.LogWarning($"[ItemLogManager] 에셋을 불러오지 못했습니다: {addressableKey}");
    //        }
    //    });
    //}
}
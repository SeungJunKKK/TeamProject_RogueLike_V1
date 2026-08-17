using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemMasterData
{
    public string ID;           
    public string Name;         
    public string Description;  
    public int Tier;            
}

public class MasterDataManager : Singleton<MasterDataManager>
{
    public Dictionary<string, ItemMasterData> ItemDictionary { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        LoadItemData();
    }

    private void LoadItemData()
    {
        ItemDictionary = new Dictionary<string, ItemMasterData>();

        TextAsset csvFile = Resources.Load<TextAsset>("ItemData");

        if (csvFile == null)
        {
            Debug.LogError("[MasterDataManager] ItemData.csv 파일을 찾을 수 없습니다!");
            return;
        }
        string[] rows = csvFile.text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < rows.Length; i++)
        {
            string line = rows[i].Replace("\r", "");
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] columns = line.Split(',');

            if (columns.Length < 4) continue;

            string rawID = columns[0].Replace("\"", "").Trim();
            string rawName = columns[1].Replace("\"", "").Trim();
            string rawDesc = columns[2].Replace("\"", "").Trim();
            string rawTierStr = columns[3].Replace("\"", "").Trim();

            if (!int.TryParse(rawTierStr, out int parsedTier))
            {
                Debug.LogError($"[MasterDataManager] 파싱 에러 ({i + 1}): ID '{rawID}'의 Tier 값인 '{rawTierStr}'은 숫자가 아닙니다!");
                continue; 
            }

            ItemMasterData newItem = new ItemMasterData
            {
                ID = rawID,
                Name = rawName,
                Description = rawDesc,
                Tier = parsedTier
            };

            if (!ItemDictionary.ContainsKey(newItem.ID))
            {
                ItemDictionary.Add(newItem.ID, newItem);
            }
            else
            {
                Debug.LogWarning($"[MasterDataManager] 중복된 아이템 ID가 존재합니다: {newItem.ID}");
            }
        }

        Debug.Log($"<color=green>[MasterDataManager] 아이템 데이터 {ItemDictionary.Count}개 로드 완료!</color>");
    }
}
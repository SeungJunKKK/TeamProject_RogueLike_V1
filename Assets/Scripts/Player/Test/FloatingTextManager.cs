using UnityEngine;
using TMPro; 

public class FloatingTextManager : MonoBehaviour
{
    [Header("Addressable Keys")]
    public string DamageTextAddress = "DamageText";

    private void OnEnable()
    {
        EventBus.Subscribe<MonsterDamagedEvent>(OnMonsterDamaged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<MonsterDamagedEvent>(OnMonsterDamaged);
    }

    private void OnMonsterDamaged(MonsterDamagedEvent e)
    {
        AddressableManager.Instance.LoadAssetAsync<GameObject>(DamageTextAddress, (prefab) =>
        {
            if (prefab != null)
            {
                GameObject textObj = PoolManager.Instance.Get(prefab, e.HitPoint, Quaternion.identity);

                PooledObject pooledObj = textObj.GetComponent<PooledObject>();
                if (pooledObj != null)
                {
                    pooledObj.Init(prefab);
                }

                TextMeshPro textMesh = textObj.GetComponent<TextMeshPro>();
                if (textMesh != null)
                {
                    textMesh.text = Mathf.Round(e.Amount).ToString();

                    if (e.IsCrit)
                    {
                        textMesh.color = Color.yellow;
                        textMesh.fontSize = 8f;
                        textMesh.fontStyle = FontStyles.Bold;
                    }
                    else
                    {
                        textMesh.color = Color.white;
                        textMesh.fontSize = 5f;
                        textMesh.fontStyle = FontStyles.Normal;
                    }
                }
            }
        });
    }
}
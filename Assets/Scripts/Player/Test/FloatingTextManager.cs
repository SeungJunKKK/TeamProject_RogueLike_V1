using UnityEngine;
using TMPro; 

public class FloatingTextManager : MonoBehaviour
{
    public GameObject DamageTextPrefab;

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
        if (DamageTextPrefab == null)
        {
            return;
        }

        GameObject textObj = PoolManager.Instance.Get(DamageTextPrefab, e.HitPoint, Quaternion.identity);

        PooledObject pooledObj = textObj.GetComponent<PooledObject>();

        if (pooledObj != null)
        {
            pooledObj.Init(DamageTextPrefab);
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
}
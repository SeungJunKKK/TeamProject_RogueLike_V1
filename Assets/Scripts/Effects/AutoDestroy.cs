using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float destroyTime = 0.3f; // 이펙트가 지속될 시간 

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}
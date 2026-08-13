using UnityEngine;


public class FloatingHealEffect : MonoBehaviour, IPoolable
{
    public float minSpeed = 5.0f; 
    public float maxSpeed = 8.0f; 

    private float m_CurrentSpeed;

    public void OnSpawn()
    {
        m_CurrentSpeed = Random.Range(minSpeed, maxSpeed);
    }

    public void OnDespawn()
    {
    }

    private void Update()
    {
        transform.Translate(Vector3.up * m_CurrentSpeed * Time.deltaTime, Space.World);
    }
}
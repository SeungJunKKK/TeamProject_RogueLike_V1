using UnityEngine;


public class FloatingHealEffect : MonoBehaviour, IPoolable
{
    public float minSpeed = 0.5f; 
    public float maxSpeed = 2.0f; 

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
using UnityEngine;
using Cinemachine;

public class AutoFindPlayer : MonoBehaviour
{
    private CinemachineVirtualCamera m_Vcam;

    void Awake()
    {
        m_Vcam = GetComponent<CinemachineVirtualCamera>();

    }

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            // 가상 카메라의 Follow와 LookAt 대상을 찾은 플레이어로 설정합니다.
            m_Vcam.Follow = player.transform;
            m_Vcam.LookAt = player.transform;
        }
        else
        {
            Debug.LogWarning("Tag가 'Player'인 오브젝트를 찾을 수 없습니다.");
        }
    }
  
}

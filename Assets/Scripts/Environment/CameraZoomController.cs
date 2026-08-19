using UnityEngine;
using Cinemachine; 

public class CameraZoomController : MonoBehaviour
{
    [Header("카메라 세팅")]
    [Tooltip("줌을 조절할 시네머신 가상 카메라를 연결하세요.")]
    [SerializeField] private CinemachineVirtualCamera m_VirtualCamera;

    [Header("줌 제한 설정")]
    [SerializeField] private float m_MinZoom = 4f;  
    [SerializeField] private float m_MaxZoom = 12f; 

    [Header("줌 감도 및 속도")]
    [SerializeField] private float m_ZoomSensitivity = 15f; 
    [SerializeField] private float m_SmoothSpeed = 10f;     

    private float m_TargetZoom;

    private void Start()
    {
       
        if (m_VirtualCamera != null)
        {
            m_TargetZoom = m_VirtualCamera.m_Lens.OrthographicSize;
        }
    }

    private void Update()
    {
        if (m_VirtualCamera == null) return;

        // 마우스 휠 입력  (위로 굴리면 넓게, 아래로 굴리면 가깝게)
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0f)
        {
            // 휠을 위로 올리면 줌 인(사이즈 감소), 아래로 내리면 줌 아웃(사이즈 증가)
            m_TargetZoom -= scrollInput * m_ZoomSensitivity;

            // 최솟값과 최댓값 사이로 목표 줌 사이즈 제한
            m_TargetZoom = Mathf.Clamp(m_TargetZoom, m_MinZoom, m_MaxZoom);
        }

        if (Mathf.Abs(m_VirtualCamera.m_Lens.OrthographicSize - m_TargetZoom) > 0.01f)
        {
            m_VirtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(
                m_VirtualCamera.m_Lens.OrthographicSize,
                m_TargetZoom,
                Time.deltaTime * m_SmoothSpeed
            );
        }
    }
}
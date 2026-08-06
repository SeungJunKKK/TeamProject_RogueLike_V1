using UnityEngine;

// 카메라 이동에 따라 배경을 시차(parallax) 이동시킨다.
// 카메라 이동량(delta) 기반이라 시작 위치가 어디든 첫 프레임에 튀지 않으며, X/Y 모두 처리한다.
public class ParallaxBackground : MonoBehaviour
{
    public GameObject cam;

    [Header("시차 효과 (0 = 고정/먼 배경, 1 = 카메라와 함께 이동/가까운 배경)")]
    public float ParallaxEffect;

    private Vector3 m_LastCamPosition;

    private void Start()
    {
        if (cam == null)
        {
            Debug.LogWarning($"[ParallaxBackground] {name}: cam이 연결되지 않았습니다.");
            return;
        }

        m_LastCamPosition = cam.transform.position;
    }

    // 카메라(Cinemachine 포함)가 이동을 마친 뒤 배경을 옮기기 위해 LateUpdate 사용
    private void LateUpdate()
    {
        if (cam == null)
        {
            return;
        }

        Vector3 delta = cam.transform.position - m_LastCamPosition;
        transform.position += new Vector3(delta.x * ParallaxEffect, delta.y * ParallaxEffect, 0f);
        m_LastCamPosition = cam.transform.position;
    }
}

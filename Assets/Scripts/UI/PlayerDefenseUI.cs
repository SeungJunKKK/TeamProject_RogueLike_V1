using System.Collections;
using UnityEngine;

public class PlayerDefenseUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SpriteRenderer shieldSpriteRenderer; // 방패 아이콘 (sEfShield_0)
    [SerializeField] private SpriteRenderer thornsSpriteRenderer; // 마법문양 배경 (sEfThorns_0)

    [Header("Thorns Rotation Effect")]
    [SerializeField] private float rotateSpeed = 90f; // 방어 중 마법문양 회전 속도

    private Coroutine blockSuccessCoroutine;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();

        // 초기 비활성화
        SetUIActive(false);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<PlayerBlockSuccessEvent>(OnBlockSuccess);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerBlockSuccessEvent>(OnBlockSuccess);
    }

    private void Update()
    {
        if (playerController == null) return;

        bool isDefending = playerController.IsDefending;

        // 방어 상태 켜짐/꺼짐 제어
        if (shieldSpriteRenderer != null && shieldSpriteRenderer.gameObject.activeSelf != isDefending)
        {
            SetUIActive(isDefending);
        }

        // 방어 중일 때 마법문양 회전 연출
        if (isDefending && thornsSpriteRenderer != null)
        {
            thornsSpriteRenderer.transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
        }
    }

    private void SetUIActive(bool active)
    {
        if (shieldSpriteRenderer != null)
            shieldSpriteRenderer.gameObject.SetActive(active);

        if (thornsSpriteRenderer != null)
            thornsSpriteRenderer.gameObject.SetActive(active);
    }

    // EventBus로 방어 성공 이벤트 수신 시 살짝 커졌다 돌아오는 펄스 연출
    private void OnBlockSuccess(PlayerBlockSuccessEvent e)
    {
        if (blockSuccessCoroutine != null)
            StopCoroutine(blockSuccessCoroutine);

        blockSuccessCoroutine = StartCoroutine(CoBlockSuccessPulse());
    }

    private IEnumerator CoBlockSuccessPulse()
    {
        if (shieldSpriteRenderer == null) yield break;

        Transform shieldTransform = shieldSpriteRenderer.transform;
        Vector3 originalScale = shieldTransform.localScale;
        Vector3 targetScale = originalScale * 1.3f; // 1.3배 확대

        // 스케일 확대
        float elapsed = 0f;
        float duration = 0.1f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            shieldTransform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }

        // 원래 크기로 복귀
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            shieldTransform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            yield return null;
        }

        shieldTransform.localScale = originalScale;
    }
}
using System.Collections;
using UnityEngine;
using TMPro;

public class StageNameDisplay : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private TextMeshProUGUI stageNameText;

    [Header("위치 설정")]
    [SerializeField] private Transform playerTransform; // 플레이어 위치
    [SerializeField] private Vector3 headOffset = new Vector3(0, 3.5f, 0); // 머리 위 여백

    [Header("연출 시간 설정")]
    [SerializeField] private float displayDuration = 2.0f; // 유지 시간
    [SerializeField] private float fadeDuration = 1.0f; // 사라지는 시간

    private Coroutine displayCoroutine;

    private void LateUpdate()
    {
        // 플레이어 머리 위 위치 실시간 추적
        if (playerTransform != null)
        {
            transform.position = playerTransform.position + headOffset;
        }
        else
        {
            // 플레이어가 스폰 시점에 생성되는 경우 태그로 실시간 탐색
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    public void ShowStageName(string stageName)
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }

        displayCoroutine = StartCoroutine(CoShowAndFade(stageName));
    }

    private IEnumerator CoShowAndFade(string stageName)
    {
        if (stageNameText != null)
        {
            stageNameText.text = stageName;
        }
        SetAlpha(1f);

        // 일정 시간 대기
        yield return new WaitForSeconds(displayDuration);

        // Fade Out
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        if (stageNameText != null)
        {
            Color c = stageNameText.color;
            stageNameText.color = new Color(c.r, c.g, c.b, alpha);
        }
    }
}
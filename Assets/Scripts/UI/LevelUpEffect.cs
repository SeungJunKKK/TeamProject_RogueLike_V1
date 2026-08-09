using System.Collections;
using UnityEngine;

public class LevelUpEffect : MonoBehaviour
{
    [Header("Sprite Animation Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] levelUpSprites; // 3개 스프라이트 에셋
    [SerializeField] private float frameRate = 0.1f;  // 각 프레임 간격
    [SerializeField] private float moveUpSpeed = 1.0f; // 위로 올라가는 속도
    [SerializeField] private float fadeDuration = 0.5f;// 서서히 사라지는 시간

    public void PlayEffect(Vector3 spawnPosition)
    {
        transform.position = spawnPosition;
        StartCoroutine(CoPlayAnimation());
    }

    private IEnumerator CoPlayAnimation()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // 3장 애니메이션 재생
        if (levelUpSprites != null && levelUpSprites.Length > 0)
        {
            for (int i = 0; i < levelUpSprites.Length; i++)
            {
                spriteRenderer.sprite = levelUpSprites[i];
                yield return new WaitForSeconds(frameRate);
            }
        }

        // 위로 떠오르며 페이드아웃 효과
        float elapsed = 0f;
        Color startColor = spriteRenderer.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            transform.position += Vector3.up * (moveUpSpeed * Time.deltaTime);

            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            yield return null;
        }

        // 이펙트 종료 후 파괴
        Destroy(gameObject);
    }
}
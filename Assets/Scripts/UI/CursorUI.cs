using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CursorUI : MonoBehaviour
{
    [Header("Cursor Settings")]
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 hotSpot = Vector2.zero;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip clickSFX;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        // 씬 전환 시 파괴되지 않는 싱글톤 세팅
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // 마우스 커서 적용
        SetCustomCursor();
    }

    private void SetCustomCursor()
    {
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
        }
    }

    private void Update()
    {
        // 마우스 좌클릭 시 UI 버튼을 눌렀는지 체크
        if (Input.GetMouseButtonDown(0))
        {
            CheckUIButtonClick();
        }
    }

    private void CheckUIButtonClick()
    {
        if (EventSystem.current == null) return;

        // 마우스 위치 아래에 있는 UI 오브젝트 탐색
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            // 클릭된 UI 또는 그 상위 부모에 Button 컴포넌트가 있는지 확인
            Button btn = result.gameObject.GetComponentInParent<Button>();
            if (btn != null && btn.interactable)
            {
                PlayClickSound();
                break;
            }
        }
    }

    public void PlayClickSound()
    {
        if (clickSFX != null && audioSource != null)
        {
            // SoundManager가 프로젝트 내에 존재한다면 SoundManager.Instance.PlaySFX(clickSFX); 사용 가능
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(clickSFX);
            }
            else
            {
                audioSource.PlayOneShot(clickSFX);
            }
        }
    }
}
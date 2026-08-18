using TMPro;
using UnityEngine;

public class ItemTextPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_NameText;
    [SerializeField] private TextMeshProUGUI m_DescText;

    [SerializeField] private float m_MoveSpeed = 1.0f;
    [SerializeField] private float m_FadeDuration = 1.2f;

    private float m_Timer = 0f;
    private Color m_NameColor;
    private Color m_DescColor;

    public void Setup(string itemName, string itemDesc, Color tierColor)
    {
        if (m_NameText != null)
        {
            m_NameText.text = itemName;
            m_NameText.color = tierColor;
            m_NameColor = tierColor;
        }

        if (m_DescText != null)
        {
            m_DescText.text = itemDesc;
            m_DescColor = m_DescText.color;
        }
    }

    private void Update()
    {
        // 위로 이동
        transform.position += Vector3.up * (m_MoveSpeed * Time.deltaTime);

        // 시간이 지남에 따라 페이드 아웃
        m_Timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, m_Timer / m_FadeDuration);

        if (m_NameText != null)
        {
            m_NameText.color = new Color(m_NameColor.r, m_NameColor.g, m_NameColor.b, alpha);
        }

        if (m_DescText != null)
        {
            m_DescText.color = new Color(m_DescColor.r, m_DescColor.g, m_DescColor.b, alpha);
        }

        if (m_Timer >= m_FadeDuration)
        {
            Destroy(gameObject);
        }
    }
}
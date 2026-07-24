using UnityEngine;
using UnityEngine.InputSystem;

public class SoundTest : MonoBehaviour
{
    [SerializeField]
    private AudioClip m_SFXClip;
    [SerializeField]
    private AudioClip m_BGMClip;

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            SoundManager.Instance.PlayBGM(m_BGMClip);
        }

        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            SoundManager.Instance.PlaySFX(m_SFXClip);
        }
    }
}

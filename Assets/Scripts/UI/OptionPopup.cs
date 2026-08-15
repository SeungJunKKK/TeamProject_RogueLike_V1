using UnityEngine;
using UnityEngine.UI;

public class OptionPopup : MonoBehaviour
{
    [SerializeField] private Slider m_MasterSlider;
    [SerializeField] private Slider m_BGMSlider;
    [SerializeField] private Slider m_SFXSlider;

    private void OnEnable()
    {
        // 저장값으로 초기화
        m_MasterSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("MasterVolume", 1f));
        m_BGMSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("BGMVolume", 1f));
        m_SFXSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("SFXVolume", 1f));

        // 코드로 soundmanager 연결
        m_MasterSlider.onValueChanged.AddListener(SoundManager.Instance.SetMasterVolume);
        m_BGMSlider.onValueChanged.AddListener(SoundManager.Instance.SetBGMVolume);
        m_SFXSlider.onValueChanged.AddListener(SoundManager.Instance.SetSFXVolume);
    }

    private void OnDisable()
    {
        m_MasterSlider.onValueChanged.RemoveListener(SoundManager.Instance.SetMasterVolume);
        m_BGMSlider.onValueChanged.RemoveListener(SoundManager.Instance.SetBGMVolume);
        m_SFXSlider.onValueChanged.RemoveListener(SoundManager.Instance.SetSFXVolume);
    }
}

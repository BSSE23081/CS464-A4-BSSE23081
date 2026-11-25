using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// Lecture 09 slides 31 to 33: a slider's 0 to 1 is not decibels. The mixer wants dB, and the ear is logarithmic:
//   dB = Log10(v) * 20      1 -> 0 dB     0.5 -> -6 dB     0.0001 -> -80 dB (silence)
// Log10(0) is minus infinity, so the slider's minimum is 0.0001, not 0.
// Put this on the always-active Canvas, NOT on the pause panel: a script on a hidden panel has not started yet.
public class VolumeSlider : MonoBehaviour
{
    public const float MinLinear = 0.0001f;

    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider slider;
    [SerializeField] string parameter = "MusicVolume";   // the exposed, renamed mixer parameter
    [SerializeField] bool isMusic = true;

    public static float ToDecibels(float linear) { return Mathf.Log10(Mathf.Max(linear, MinLinear)) * 20f; }

    void Start()
    {
        float saved = isMusic ? SaveData.MusicVolume : SaveData.SfxVolume;
        slider.minValue = MinLinear;
        slider.maxValue = 1f;
        slider.SetValueWithoutNotify(saved);     // show the saved value without "changing" it
        Apply(saved);
        slider.onValueChanged.AddListener(OnChanged);
    }

    void OnDestroy()
    {
        if (slider != null) slider.onValueChanged.RemoveListener(OnChanged);
    }

    void OnChanged(float value)
    {
        Apply(value);
        if (isMusic) SaveData.MusicVolume = value;
        else SaveData.SfxVolume = value;
    }

    void Apply(float value)
    {
        if (mixer != null) mixer.SetFloat(parameter, ToDecibels(value));
    }
}

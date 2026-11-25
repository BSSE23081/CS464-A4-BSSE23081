using UnityEngine;
using UnityEngine.Audio;

// Lecture 10 slides 23 to 25: one music player that outlives every scene load.
// The singleton check is what keeps three trips to the menu from stacking three tracks.
public class PersistentMusic : MonoBehaviour
{
    public static PersistentMusic Instance { get; private set; }

    [SerializeField] AudioSource source;    // loop on, Output = Music, Play On Awake OFF
    [SerializeField] AudioMixer mixer;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (Instance != this) return;          // the copy that is about to be destroyed stays silent
        ApplySavedVolumes(mixer);              // the mixer is set in Start: SetFloat in Awake can be too early
        source.loop = true;
        if (!source.isPlaying) source.Play();
    }

    public static void ApplySavedVolumes(AudioMixer m)
    {
        if (m == null) return;
        m.SetFloat("MusicVolume", VolumeSlider.ToDecibels(SaveData.MusicVolume));
        m.SetFloat("SfxVolume", VolumeSlider.ToDecibels(SaveData.SfxVolume));
    }

    void OnDestroy() { if (Instance == this) Instance = null; }
}

using UnityEngine;

// Lecture 10 slides 15 to 17 and 21: a sound with a PLACE. Spatial Blend 1 makes it fully 3D (0 is 2D, heard the same
// everywhere), and the rolloff decides how it fades with distance:
//   Logarithmic: -6 dB for every doubling of distance, never quite silent (cut off at Max Distance)
//   Linear:      falls in a straight line and is silent at Max Distance
// Min Distance is the size of the sound (full volume inside it); Max Distance is where the fade ends.
[RequireComponent(typeof(AudioSource))]
public class WorldSound : MonoBehaviour
{
    [SerializeField] AudioRolloffMode rolloff = AudioRolloffMode.Logarithmic;
    [SerializeField] float minDistance = 2f;
    [SerializeField] float maxDistance = 16f;
    [SerializeField, Range(0f, 1f)] float volume = 1f;

    void Awake()
    {
        AudioSource s = GetComponent<AudioSource>();
        s.spatialBlend = 1f;
        s.rolloffMode = rolloff;
        s.minDistance = minDistance;
        s.maxDistance = maxDistance;
        s.volume = volume;
        s.dopplerLevel = 0f;       // a moving guard should hum, not bend in pitch
        s.loop = true;
        if (!s.isPlaying) s.Play();
    }
}

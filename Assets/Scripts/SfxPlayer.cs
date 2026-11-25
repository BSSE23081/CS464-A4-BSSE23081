using UnityEngine;

public enum Sfx { Pickup, Hit, Button, Jump, Bounce, Checkpoint, PowerUp, Win, Lose }

// Lecture 09's SfxPlayer: one singleton, a few AudioSources on the SFX mixer group, PlayOneShot with a random pitch.
// It lives on its own root object, so a clip keeps playing when the coin or the enemy that asked for it is destroyed.
public class SfxPlayer : MonoBehaviour
{
    public static SfxPlayer Instance { get; private set; }

    [SerializeField] AudioSource[] sources;        // four, all with Output = SFX
    [SerializeField] float pitchSemitones = 2f;    // random pitch: +/- 2 semitones
    [SerializeField] AudioClip pickup, hit, button, jump, bounce, checkpoint, powerUp, win, lose;

    int next;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }   // the singleton check: the first one stays
        Instance = this;
        DontDestroyOnLoad(gameObject);
        for (int i = 0; i < sources.Length; i++)
        {
            sources[i].playOnAwake = false;
            sources[i].ignoreListenerPause = true;   // a menu click still plays while the game is paused
        }
    }

    // A pitch that is "n semitones" up is a frequency ratio of 2^(n / 12).
    public static float SemitonesToPitch(float semitones) { return Mathf.Pow(2f, semitones / 12f); }

    // SfxPlayer.Instance.Play(clip): PlayOneShot on the next source, at a slightly different pitch each time.
    public void Play(AudioClip clip, float volume = 1f, bool randomPitch = true)
    {
        if (clip == null || sources == null || sources.Length == 0) return;
        AudioSource s = sources[next];
        next = (next + 1) % sources.Length;
        s.pitch = randomPitch ? SemitonesToPitch(Random.Range(-pitchSemitones, pitchSemitones)) : 1f;
        s.PlayOneShot(clip, volume);
    }

    // The one-line call the other scripts use: SfxPlayer.Play(Sfx.Pickup).
    public static void Play(Sfx id, float volume = 1f)
    {
        if (Instance == null) return;
        Instance.Play(Instance.Clip(id), volume, id != Sfx.Win && id != Sfx.Lose);   // jingles keep their pitch
    }

    AudioClip Clip(Sfx id)
    {
        switch (id)
        {
            case Sfx.Pickup: return pickup;
            case Sfx.Hit: return hit;
            case Sfx.Button: return button;
            case Sfx.Jump: return jump;
            case Sfx.Bounce: return bounce;
            case Sfx.Checkpoint: return checkpoint;
            case Sfx.PowerUp: return powerUp;
            case Sfx.Win: return win;
            default: return lose;
        }
    }

    void OnDestroy() { if (Instance == this) Instance = null; }
}

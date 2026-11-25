using UnityEngine;
using UnityEngine.UI;

// Lecture 08 slides 24 to 26. A health bar is two Images, both Image Type Filled / Horizontal / Origin Left:
// the FILL (coloured by a Gradient at the same fraction) and a paler LAG bar behind it that shows what was just lost.
// Listener 1 of 5 for Health's events: += in OnEnable, -= in OnDisable.
public class HealthBar : MonoBehaviour
{
    [SerializeField] Image fill;
    [SerializeField] Image lag;
    [SerializeField] Health health;
    public Gradient colour;                     // red to amber to green; the builder fills it with DefaultGradient()
    [SerializeField] float lagDelay = 0.4f;     // seconds before the lag bar starts to catch up
    [SerializeField] float lagSpeed = 0.8f;     // fill per second it catches up at

    float target = 1f;
    float lagStartsAt;

    public static Gradient DefaultGradient()
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new[]
            {
                new GradientColorKey(Palette.Hazard, 0f),
                new GradientColorKey(ColourUtil.Hsv(40f, 0.75f, 0.95f), 0.5f),     // amber
                new GradientColorKey(Palette.Pickup, 1f)
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        return g;
    }

    void OnEnable()
    {
        if (health == null) health = FindFirstObjectByType<Health>();
        if (health == null) return;
        health.Damaged += OnChanged;
        health.Healed += OnChanged;
    }

    void OnDisable()
    {
        if (health == null) return;
        health.Damaged -= OnChanged;
        health.Healed -= OnChanged;
    }

    // Start, not OnEnable: every Awake has run by now, so Health.Current is real.
    void Start()
    {
        if (health != null) Set(health.Current, health.Max);
    }

    void OnChanged(int current, int max) { Set(current, max); }

    // The whole health bar. The cast makes it float division: two ints would give 4 / 5 = 0 (slide 46).
    public void Set(int current, int max)
    {
        float next = Mathf.Clamp01((float)current / max);
        if (next < target) lagStartsAt = Time.unscaledTime + lagDelay;   // a hit: the lag bar waits
        else lag.fillAmount = next;                                       // a heal: it moves up with the fill
        target = next;
        fill.fillAmount = target;
        fill.color = colour.Evaluate(target);
    }

    // Unscaled time, so the bar finishes its slide even if the game is paused on the same frame.
    void Update()
    {
        if (Time.unscaledTime < lagStartsAt) return;
        lag.fillAmount = Mathf.MoveTowards(lag.fillAmount, target, lagSpeed * Time.unscaledDeltaTime);
    }
}

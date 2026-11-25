using UnityEngine;

// Lecture 07 slide 35: hearts, i-frames, and "down at <= 0, never == 0".
// Health owns the hearts; the GameManager owns the lives.
public class Health : MonoBehaviour
{
    [SerializeField] int max = 3;
    [SerializeField] float iFrames = 1f;   // seconds nothing can hurt you after a hit
    public bool shielded;                  // Lecture 07's shield: a bool TakeDamage checks

    // Lecture 09: Health announces what happened and does not know who is listening.
    // Listeners subscribe in OnEnable and unsubscribe in OnDisable (HealthBar, HitFlash, DamageSfx, HitSparks, CameraShake).
    public event System.Action<int, int> Damaged;   // (current, max) after a hit that landed
    public event System.Action<int, int> Healed;    // (current, max) after a heal or a respawn
    public event System.Action Died;

    int current;
    float invulnerableUntil;
    Renderer[] renderers;

    public int Current => current;
    public int Max => max;
    public bool IsInvulnerable => Time.time < invulnerableUntil;

    void Awake()
    {
        current = max;
        renderers = GetComponentsInChildren<Renderer>();
    }

    // Returns true only if the hit landed, so the hazard knows whether to knock back.
    public bool TakeDamage(int amount)
    {
        if (current <= 0 || Time.time < invulnerableUntil) return false;   // i-frames: one hit per iFrames, not 50 a second
        invulnerableUntil = Time.time + iFrames;                           // set first, so a hit that pops the shield also buys safety

        if (shielded)
        {
            shielded = false;
            Refresh();
            if (GameManager.Instance != null) GameManager.Instance.ShowMessage("Shield absorbed the hit");
            return false;
        }

        current -= amount;
        Damaged?.Invoke(current, max);
        Refresh();
        if (current <= 0)                                                                       // <= 0: a hit of 2 can skip 0
        {
            Died?.Invoke();
            if (GameManager.Instance != null) GameManager.Instance.PlayerDied();
        }
        return true;
    }

    // Called by the GameManager on a respawn: full hearts and a moment of protection.
    public void Restore(float protectionSeconds)
    {
        current = max;
        invulnerableUntil = Time.time + protectionSeconds;
        Healed?.Invoke(current, max);
        Refresh();
    }

    void Refresh()
    {
        if (GameManager.Instance != null) GameManager.Instance.UpdateUI();
    }

    // Blink while protected, so the player can see the i-frames.
    void Update()
    {
        bool visible = Time.time >= invulnerableUntil || ((int)(Time.time * 12f) % 2 == 0);
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].enabled = visible;
    }
}

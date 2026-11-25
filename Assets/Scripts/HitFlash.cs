using UnityEngine;
using UnityEngine.UI;

// Listener 2: a red flash over the screen when the player is hurt (Lecture 08's "meta" UI: in the story, not in the world).
// The Image has Raycast Target OFF: an invisible full-screen Image that catches clicks is the classic dead-button bug.
public class HitFlash : MonoBehaviour
{
    [SerializeField] Image flash;
    [SerializeField] Health health;
    [SerializeField] float peakAlpha = 0.35f;
    [SerializeField] float fadeSeconds = 0.35f;

    float alpha;

    void OnEnable()
    {
        if (health == null) health = FindFirstObjectByType<Health>();
        if (health != null) health.Damaged += OnHit;
    }

    void OnDisable()
    {
        if (health != null) health.Damaged -= OnHit;
    }

    void OnHit(int current, int max) { alpha = peakAlpha; }

    void Update()
    {
        if (alpha <= 0f) return;
        alpha = Mathf.MoveTowards(alpha, 0f, peakAlpha / fadeSeconds * Time.unscaledDeltaTime);
        Color c = flash.color;
        c.a = alpha;
        flash.color = c;
    }
}

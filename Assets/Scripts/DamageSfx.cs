using UnityEngine;

// Listener 3: a hit you can hear. Lives on the player, next to Health.
public class DamageSfx : MonoBehaviour
{
    Health health;

    void OnEnable()
    {
        health = GetComponent<Health>();
        health.Damaged += OnHit;
    }

    void OnDisable()
    {
        if (health != null) health.Damaged -= OnHit;
    }

    void OnHit(int current, int max) { SfxPlayer.Play(Sfx.Hit); }
}

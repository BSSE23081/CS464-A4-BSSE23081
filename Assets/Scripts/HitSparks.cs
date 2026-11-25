using UnityEngine;

// Listener 4: a hit you can see. One burst of sparks, no loop. The ParticleSystem is a child of the player
// (Simulation Space: World, so the sparks stay where the hit happened while the ball rolls on).
public class HitSparks : MonoBehaviour
{
    [SerializeField] ParticleSystem sparks;
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

    void OnHit(int current, int max)
    {
        if (sparks != null) sparks.Play();
    }
}

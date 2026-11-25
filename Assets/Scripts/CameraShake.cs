using UnityEngine;

// Listener 5: a short shake of the camera on a hit. It only sets an offset on the CameraFollow, which adds it
// after its own smoothing, so the shake never fights the follow.
[RequireComponent(typeof(CameraFollow))]
public class CameraShake : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] float strength = 0.35f;     // metres at the start
    [SerializeField] float seconds = 0.3f;

    CameraFollow follow;
    float shakeEndsAt;

    void Awake() { follow = GetComponent<CameraFollow>(); }

    void OnEnable()
    {
        if (health == null) health = FindFirstObjectByType<Health>();
        if (health != null) health.Damaged += OnHit;
    }

    void OnDisable()
    {
        if (health != null) health.Damaged -= OnHit;
    }

    void OnHit(int current, int max) { shakeEndsAt = Time.unscaledTime + seconds; }

    void LateUpdate()
    {
        float left = shakeEndsAt - Time.unscaledTime;
        follow.shakeOffset = left > 0f ? Random.insideUnitSphere * (strength * left / seconds) : Vector3.zero;
    }
}

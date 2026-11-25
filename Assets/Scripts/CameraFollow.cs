using UnityEngine;

// A fixed-angle follow camera. Frame-rate independent smoothing: t = 1 - exp(-k * dt)  (Lecture 05).
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 11f, -9f);
    public float smooth = 6f;
    public float lowestFollowY = -1f;   // the camera does not chase you down into the void
    [HideInInspector] public Vector3 shakeOffset;   // set by CameraShake, added after the smoothing

    Vector3 smoothed;

    void Start()
    {
        if (target == null)
        {
            PlayerController pc = FindAnyObjectByType<PlayerController>();
            if (pc != null) target = pc.transform;
        }
        transform.rotation = Quaternion.LookRotation(-offset.normalized);
        if (target != null) { smoothed = Anchor() + offset; transform.position = smoothed; }
    }

    Vector3 Anchor()
    {
        Vector3 p = target.position;
        p.y = Mathf.Max(p.y, lowestFollowY);
        return p;
    }

    void LateUpdate()
    {
        if (target == null) return;
        float t = 1f - Mathf.Exp(-smooth * Time.deltaTime);
        smoothed = Vector3.Lerp(smoothed, Anchor() + offset, t);   // smooth the real position, not the shaken one
        transform.position = smoothed + shakeOffset;
    }
}

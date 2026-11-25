using UnityEngine;

// A trigger you pass through: respawn here from now on (Assignment 03 page 4).
public class Checkpoint : MonoBehaviour
{
    [SerializeField] Renderer flag;
    bool reached;

    void OnTriggerEnter(Collider other)
    {
        if (reached || !other.CompareTag("Player")) return;
        reached = true;
        GameManager.Instance.SetCheckpoint(transform);
        SfxPlayer.Play(Sfx.Checkpoint);
        if (flag != null) flag.material.color = Palette.Pickup;   // the "good" colour: reached
    }
}

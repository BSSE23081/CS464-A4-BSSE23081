using UnityEngine;

// The scene's ONE AudioListener. The follow camera is about 14 m from the ball, so a listener on the camera
// would hear every sound from that far away. This one sits on the player, but turns with the camera,
// so "left" in the headphones is still left on the screen.
public class ListenerOnPlayer : MonoBehaviour
{
    [SerializeField] Transform player;
    Transform cam;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (Camera.main != null) cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (player != null) transform.position = player.position;
        if (cam != null) transform.rotation = cam.rotation;
    }
}

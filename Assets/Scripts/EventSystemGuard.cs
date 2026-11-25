using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif

// Lecture 08, slide 57: "a button nobody can click" is most often a missing EventSystem, or one whose input module
// has no input to read. This makes sure there is exactly the right one, in every scene, before anyone clicks.
public class EventSystemGuard : MonoBehaviour
{
    void Awake()
    {
        EventSystem es = FindFirstObjectByType<EventSystem>();
        if (es == null) es = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // New Input System only: the UI module needs its default actions, or clicks and arrows do nothing.
        InputSystemUIInputModule module = es.GetComponent<InputSystemUIInputModule>();
        if (module == null)
        {
            StandaloneInputModule old = es.GetComponent<StandaloneInputModule>();
            if (old != null) Destroy(old);            // it would throw: it reads the old Input class
            module = es.gameObject.AddComponent<InputSystemUIInputModule>();
        }
        if (module.actionsAsset == null) module.AssignDefaultActions();
#else
        if (es.GetComponent<BaseInputModule>() == null) es.gameObject.AddComponent<StandaloneInputModule>();
#endif
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// A click sound on every Button under this canvas, including the ones on hidden panels.
public class ButtonSfx : MonoBehaviour
{
    readonly List<Button> buttons = new List<Button>();

    void Start()
    {
        GetComponentsInChildren(true, buttons);
        for (int i = 0; i < buttons.Count; i++) buttons[i].onClick.AddListener(Click);
    }

    void OnDestroy()
    {
        for (int i = 0; i < buttons.Count; i++)
            if (buttons[i] != null) buttons[i].onClick.RemoveListener(Click);
    }

    static void Click() { SfxPlayer.Play(Sfx.Button); }
}

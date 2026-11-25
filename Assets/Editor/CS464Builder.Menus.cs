using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The two scenes that are not levels: MainMenu (build index 0) and Credits (build index 6).
public static partial class CS464Builder
{
    static GameObject pSfx, pMusic, pLoader;

    static void AddEventSystem()
    {
        GameObject es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // New Input System only: the UI module needs its default actions assigned, or clicks do nothing.
        Component module = es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        System.Reflection.MethodInfo assign = module.GetType().GetMethod("AssignDefaultActions");
        if (assign != null) assign.Invoke(module, null);
#else
        es.AddComponent<StandaloneInputModule>();
#endif
    }

    // What every scene shares: the audio players and the loader (each a singleton or a per-scene object).
    static void AddSceneBasics()
    {
        Inst(pSfx, null, Vector3.zero);
        Inst(pMusic, null, Vector3.zero);
        Inst(pLoader, null, Vector3.zero);
    }

    static void UiCamera()
    {
        Camera cam = Object.FindFirstObjectByType<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Palette.Panel;
    }

    static GameObject MenuCanvas(string name)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        SetCanvas(root, 0);
        root.AddComponent<EventSystemGuard>();
        root.AddComponent<ButtonSfx>();
        return root;
    }

    static GameObject Group2(Transform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Stretch(go);
        return go;
    }

    // ------------------------------------------------------------------ MainMenu
    static void BuildMainMenuScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        UiCamera();
        AddSceneBasics();

        GameObject canvas = MenuCanvas("MenuCanvas");
        Transform t = canvas.transform;
        MainMenu menu = canvas.AddComponent<MainMenu>();
        LevelSelect select = canvas.AddComponent<LevelSelect>();

        // main panel
        GameObject main = Group2(t, "MainPanel");
        Label(main.transform, "Title", "COLLECT, DODGE, SURVIVE", 84f, TextAlignmentOptions.Center, Palette.Text, MID, new Vector2(0f, 380f), new Vector2(1200f, 140f));
        Button play = Btn(main.transform, "PlayButton", "Play", MID, new Vector2(0f, 210f), new Vector2(520f, 100f), 40f, menu.Play);
        Btn(main.transform, "LevelSelectButton", "Level select", MID, new Vector2(0f, 90f), new Vector2(520f, 100f), 40f, menu.ShowLevelSelect);
        Btn(main.transform, "CreditsButton", "Credits", MID, new Vector2(0f, -30f), new Vector2(520f, 100f), 40f, menu.Credits);
        Btn(main.transform, "QuitButton", "Quit", MID, new Vector2(0f, -150f), new Vector2(520f, 100f), 40f, menu.Quit);
        Btn(main.transform, "ResetButton", "Reset progress", MID, new Vector2(0f, -250f), new Vector2(360f, 64f), 28f, menu.ResetProgress);

        Label(main.transform, "MusicLabel", "Music", 32f, TextAlignmentOptions.Left, Palette.Text, MID, new Vector2(-460f, -350f), new Vector2(240f, 50f));
        Slider music = MakeSlider(main.transform, "MusicSlider", MID, new Vector2(-60f, -350f), new Vector2(520f, 50f));
        Label(main.transform, "SfxLabel", "Effects", 32f, TextAlignmentOptions.Left, Palette.Text, MID, new Vector2(-460f, -420f), new Vector2(240f, 50f));
        Slider sfx = MakeSlider(main.transform, "SfxSlider", MID, new Vector2(-60f, -420f), new Vector2(520f, 50f));

        // The sliders' scripts sit on the Canvas, which is always active.
        VolumeSlider vm = canvas.AddComponent<VolumeSlider>();
        SetProp(vm, "mixer", mixer);
        SetProp(vm, "slider", music);
        SetProp(vm, "parameter", "MusicVolume");
        SetProp(vm, "isMusic", true);
        VolumeSlider vs = canvas.AddComponent<VolumeSlider>();
        SetProp(vs, "mixer", mixer);
        SetProp(vs, "slider", sfx);
        SetProp(vs, "parameter", "SfxVolume");
        SetProp(vs, "isMusic", false);

        // level select panel
        GameObject lv = Group2(t, "LevelPanel");
        Label(lv.transform, "Title", "LEVEL SELECT", 90f, TextAlignmentOptions.Center, Palette.Text, MID, new Vector2(0f, 300f), new Vector2(1200f, 120f));
        Button[] buttons = new Button[SaveData.LevelCount];
        TMP_Text[] labels = new TMP_Text[SaveData.LevelCount];
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i] = Btn(lv.transform, "Level" + (i + 1) + "Button", "Level " + (i + 1), MID, new Vector2(-460f + i * 230f, 60f), new Vector2(210f, 170f), 34f, null);
            labels[i] = buttons[i].transform.Find("Label").GetComponent<TMP_Text>();
        }
        Btn(lv.transform, "BackButton", "Back", MID, new Vector2(0f, -200f), new Vector2(420f, 100f), 38f, menu.ShowMain);
        SetArray(select, "buttons", buttons);
        SetArray(select, "labels", labels);

        SetProp(menu, "mainPanel", main);
        SetProp(menu, "levelPanel", lv);
        SetProp(menu, "playButton", play.gameObject);
        SetProp(menu, "levelSelect", select);

        AddEventSystem();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SceneDir + "/MainMenu.unity");
    }

    // ------------------------------------------------------------------ Credits
    const string CreditsText =
        "SOUND EFFECTS (sfx_pickup, sfx_hit, sfx_button, sfx_jump, sfx_bounce, sfx_checkpoint, sfx_powerup, sfx_win, sfx_lose)\n" +
        "Synthesised by a script, no samples: Assets/Tools/gen_audio.py (Python + numpy)  |  CC0 / public domain\n\n" +
        "WORLD SOUNDS (loop_guard_hum, loop_goal_chime, loop_hazard_buzz)\n" +
        "Synthesised by the same script  |  CC0 / public domain\n\n" +
        "MUSIC (music_theme, a 24-second loop)\n" +
        "Synthesised by the same script  |  CC0 / public domain\n\n" +
        "FONT (the TextMesh Pro default)\n" +
        "Liberation Sans, Red Hat, Inc.  |  SIL Open Font License 1.1\n\n" +
        "Game engine: Unity 6.3";

    static void BuildCreditsScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        UiCamera();
        AddSceneBasics();

        GameObject canvas = MenuCanvas("CreditsCanvas");
        Transform t = canvas.transform;
        CreditsScreen cs = canvas.AddComponent<CreditsScreen>();
        Label(t, "Title", "CREDITS", 90f, TextAlignmentOptions.Center, Palette.Text, MID, new Vector2(0f, 400f), new Vector2(1200f, 120f));
        TextMeshProUGUI body = Label(t, "Body", CreditsText, 30f, TextAlignmentOptions.TopLeft, Palette.Text, MID, new Vector2(0f, 90f), new Vector2(1200f, 640f));
        body.textWrappingMode = TextWrappingModes.Normal;
        Button back = Btn(t, "BackButton", "Back to menu", MID, new Vector2(0f, -400f), new Vector2(520f, 100f), 38f, cs.Back);
        SetProp(cs, "backButton", back.gameObject);

        AddEventSystem();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SceneDir + "/Credits.unity");
    }
}

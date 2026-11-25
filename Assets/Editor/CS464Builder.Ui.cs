using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// The interface: one HUD prefab for every level (health bar, score, lives, objective, timer, message, pause menu,
// win and game-over screens) and one LevelLoader prefab with the loading bar. Colours come from Palette only.
public static partial class CS464Builder
{
    static readonly Vector2 TL = new Vector2(0f, 1f), TR = new Vector2(1f, 1f), TC = new Vector2(0.5f, 1f);
    static readonly Vector2 BL = new Vector2(0f, 0f), BC = new Vector2(0.5f, 0f), MID = new Vector2(0.5f, 0.5f);

    static Sprite UiSprite() { return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"); }
    static Sprite KnobSprite() { return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"); }

    static RectTransform Place(GameObject go, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    static void Stretch(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static Image Img(Transform parent, string name, Color color, bool raycast)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image im = go.GetComponent<Image>();
        im.color = color;
        im.raycastTarget = raycast;     // decoration never catches a click (Lecture 08, slide 57)
        return im;
    }

    // A SOLID panel in the palette's dark: every HUD text sits on one, so its contrast can be measured.
    static GameObject SolidPanel(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        Image im = Img(parent, name, Palette.Panel, false);
        Place(im.gameObject, anchor, pos, size);
        return im.gameObject;
    }

    static TextMeshProUGUI Label(Transform parent, string name, string text, float size, TextAlignmentOptions align, Color color, Vector2 anchor, Vector2 pos, Vector2 box)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.font = TMP_Settings.defaultFontAsset;
        t.text = text;
        t.fontSize = size;
        t.alignment = align;
        t.color = color;
        t.raycastTarget = false;
        t.enableAutoSizing = true;                 // shrinks to fit the box: text can never be cut off at the screen edge
        t.fontSizeMin = Mathf.Max(14f, size * 0.45f);
        t.fontSizeMax = size;
        Place(go, anchor, pos, box);
        return t;
    }

    static void Fill(Image im, bool useSprite)
    {
        im.sprite = UiSprite();
        im.type = Image.Type.Filled;
        im.fillMethod = Image.FillMethod.Horizontal;
        im.fillOrigin = (int)Image.OriginHorizontal.Left;
        im.fillAmount = 1f;
    }

    // A button with the five visual states, and a dark label on all of them (>= 4.5 : 1, measured).
    static Button Btn(Transform parent, string name, string label, Vector2 anchor, Vector2 pos, Vector2 size, float fontSize, UnityAction onClick)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Image im = go.GetComponent<Image>();
        im.color = Color.white;                       // the Button's colour tints this, so the block holds the real colours
        Place(go, anchor, pos, size);
        Button b = go.GetComponent<Button>();
        b.targetGraphic = im;
        ColorBlock cb = b.colors;
        cb.normalColor = Palette.Button;
        cb.highlightedColor = Palette.ButtonHighlighted;
        cb.selectedColor = Palette.ButtonSelected;    // the keyboard cursor: the lightest state
        cb.pressedColor = Palette.ButtonPressed;
        cb.disabledColor = Palette.ButtonDisabled;
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.08f;
        b.colors = cb;
        Label(go.transform, "Label", label, fontSize, TextAlignmentOptions.Center, Palette.Panel, Vector2.zero, Vector2.zero, Vector2.zero);
        Stretch(go.transform.Find("Label").gameObject);
        if (onClick != null) UnityEventTools.AddPersistentListener(b.onClick, onClick);
        return b;
    }

    // Slider: Background, Fill Area > Fill, Handle Slide Area > Handle, wired the way GameObject > UI > Slider does it.
    static Slider MakeSlider(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(Slider));
        root.transform.SetParent(parent, false);
        Place(root, anchor, pos, size);

        Image bg = Img(root.transform, "Background", Palette.ButtonDisabled, true);
        bg.rectTransform.anchorMin = new Vector2(0f, 0.3f);
        bg.rectTransform.anchorMax = new Vector2(1f, 0.7f);
        bg.rectTransform.offsetMin = Vector2.zero;
        bg.rectTransform.offsetMax = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(root.transform, false);
        RectTransform fa = fillArea.GetComponent<RectTransform>();
        fa.anchorMin = new Vector2(0f, 0.3f);
        fa.anchorMax = new Vector2(1f, 0.7f);
        fa.offsetMin = new Vector2(8f, 0f);
        fa.offsetMax = new Vector2(-8f, 0f);
        Image fill = Img(fillArea.transform, "Fill", Palette.Pickup, false);
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = Vector2.one;
        fill.rectTransform.sizeDelta = new Vector2(10f, 0f);

        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(root.transform, false);
        RectTransform ha = handleArea.GetComponent<RectTransform>();
        ha.anchorMin = Vector2.zero;
        ha.anchorMax = Vector2.one;
        ha.offsetMin = new Vector2(16f, 0f);
        ha.offsetMax = new Vector2(-16f, 0f);
        Image handle = Img(handleArea.transform, "Handle", Color.white, true);
        handle.sprite = KnobSprite();
        handle.rectTransform.anchorMin = Vector2.zero;
        handle.rectTransform.anchorMax = Vector2.one;
        handle.rectTransform.sizeDelta = new Vector2(32f, 0f);

        Slider s = root.GetComponent<Slider>();
        s.fillRect = fill.rectTransform;
        s.handleRect = handle.rectTransform;
        s.targetGraphic = handle;
        s.direction = Slider.Direction.LeftToRight;
        s.minValue = VolumeSlider.MinLinear;
        s.maxValue = 1f;
        s.value = 1f;
        ColorBlock cb = s.colors;
        cb.normalColor = Palette.Button;
        cb.highlightedColor = Palette.ButtonHighlighted;
        cb.selectedColor = Palette.ButtonSelected;
        cb.pressedColor = Palette.ButtonPressed;
        cb.disabledColor = Palette.ButtonDisabled;
        s.colors = cb;
        return s;
    }

    // A dimmed full-screen overlay (it swallows clicks meant for the game) with a solid card in the middle.
    static GameObject OverlayScreen(Transform parent, string name, Vector2 cardSize, out Transform card)
    {
        Image overlay = Img(parent, name, new Color(0f, 0f, 0f, 0.65f), true);
        Stretch(overlay.gameObject);
        Image c = Img(overlay.transform, "Card", Palette.Panel, false);
        Place(c.gameObject, MID, Vector2.zero, cardSize);
        card = c.transform;
        return overlay.gameObject;
    }

    static void SetCanvas(GameObject root, int sortOrder)
    {
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;   // so the HUD is the same SIZE on every screen
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    // ------------------------------------------------------------------ the HUD prefab
    static void BuildHud()
    {
        GameObject root = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        SetCanvas(root, 0);
        Transform t = root.transform;
        PauseMenu pause = root.AddComponent<PauseMenu>();
        LevelButtons nav = root.AddComponent<LevelButtons>();
        root.AddComponent<EventSystemGuard>();
        root.AddComponent<ButtonSfx>();

        // Meta UI: a red flash over the whole screen when you are hurt. Raycast Target OFF.
        Image flash = Img(t, "HitFlash", new Color(Palette.Hazard.r, Palette.Hazard.g, Palette.Hazard.b, 0f), false);
        Stretch(flash.gameObject);
        HitFlash hf = root.AddComponent<HitFlash>();
        SetProp(hf, "flash", flash);

        // Top left: the health bar (fill + lag) and the lives.
        GameObject hp = SolidPanel(t, "HealthPanel", TL, new Vector2(24f, -24f), new Vector2(440f, 118f));
        Image back = Img(hp.transform, "BarBack", ColourUtil.Hsv(Palette.BaseHue, 0.30f, 0.32f), false);
        Place(back.gameObject, TL, new Vector2(20f, -18f), new Vector2(400f, 34f));
        Image lag = Img(back.transform, "Lag", ColourUtil.Hsv(Palette.BaseHue, 0.12f, 0.78f), false);
        Stretch(lag.gameObject);
        Fill(lag, true);
        Image fill = Img(back.transform, "Fill", Palette.Pickup, false);
        Stretch(fill.gameObject);
        Fill(fill, true);
        Label(hp.transform, "LivesText", "Lives: 3", 38f, TextAlignmentOptions.TopLeft, Palette.Text, TL, new Vector2(20f, -64f), new Vector2(400f, 44f));
        HealthBar bar = hp.AddComponent<HealthBar>();
        SetProp(bar, "fill", fill);
        SetProp(bar, "lag", lag);
        bar.colour = HealthBar.DefaultGradient();
        EditorUtility.SetDirty(bar);

        // Top right: the score, in the pickup colour.
        GameObject sp = SolidPanel(t, "ScorePanel", TR, new Vector2(-24f, -24f), new Vector2(440f, 78f));
        Label(sp.transform, "ScoreText", "Coins: 0", 42f, TextAlignmentOptions.Center, Palette.Pickup, MID, Vector2.zero, new Vector2(420f, 60f));

        // Top centre: the clock (only shown on timed levels).
        GameObject tp = SolidPanel(t, "TimerPanel", TC, new Vector2(0f, -24f), new Vector2(260f, 78f));
        Label(tp.transform, "TimerText", "", 52f, TextAlignmentOptions.Center, Palette.Text, MID, Vector2.zero, new Vector2(240f, 70f));
        tp.SetActive(false);

        // Bottom left: the objective.
        GameObject op = SolidPanel(t, "ObjectivePanel", BL, new Vector2(24f, 24f), new Vector2(760f, 64f));
        Label(op.transform, "ObjectiveText", "", 30f, TextAlignmentOptions.Left, Palette.Text, MID, Vector2.zero, new Vector2(720f, 56f));

        // Bottom centre: messages (the intro, "Checkpoint!") above the pause button.
        GameObject mp = SolidPanel(t, "MessagePanel", BC, new Vector2(0f, 112f), new Vector2(1100f, 150f));
        Label(mp.transform, "MessageText", "", 34f, TextAlignmentOptions.Center, Palette.Text, MID, Vector2.zero, new Vector2(1060f, 140f));
        mp.SetActive(false);

        Button pauseButton = Btn(t, "PauseButton", "Pause (Esc)", BC, new Vector2(0f, 24f), new Vector2(300f, 64f), 30f, pause.Pause);
        pauseButton.GetComponent<RectTransform>().pivot = BC;

        // ---- pause screen: Resume, Restart, Quit to menu, and the two volume sliders
        Transform card;
        GameObject pauseScreen = OverlayScreen(t, "PausePanel", new Vector2(820f, 820f), out card);
        Label(card, "Title", "PAUSED", 80f, TextAlignmentOptions.Center, Palette.Text, MID, new Vector2(0f, 320f), new Vector2(800f, 110f));
        Button resume = Btn(card, "ResumeButton", "Resume", MID, new Vector2(0f, 190f), new Vector2(560f, 100f), 38f, pause.Resume);
        Btn(card, "RestartButton", "Restart", MID, new Vector2(0f, 62f), new Vector2(560f, 100f), 38f, pause.Restart);
        Btn(card, "QuitButton", "Quit to menu", MID, new Vector2(0f, -66f), new Vector2(560f, 100f), 38f, pause.QuitToMenu);
        Label(card, "MusicLabel", "Music", 32f, TextAlignmentOptions.Left, Palette.Text, MID, new Vector2(-300f, -210f), new Vector2(240f, 50f));
        Slider music = MakeSlider(card, "MusicSlider", MID, new Vector2(100f, -210f), new Vector2(520f, 50f));
        Label(card, "SfxLabel", "Effects", 32f, TextAlignmentOptions.Left, Palette.Text, MID, new Vector2(-300f, -290f), new Vector2(240f, 50f));
        Slider sfx = MakeSlider(card, "SfxSlider", MID, new Vector2(100f, -290f), new Vector2(520f, 50f));
        SetProp(pause, "pausePanel", pauseScreen);
        SetProp(pause, "firstButton", resume.gameObject);
        pauseScreen.SetActive(false);

        // VolumeSliders live on the always-active Canvas root, NOT on the hidden pause panel (its scripts would not have started).
        VolumeSlider vm = root.AddComponent<VolumeSlider>();
        SetProp(vm, "mixer", mixer);
        SetProp(vm, "slider", music);
        SetProp(vm, "parameter", "MusicVolume");
        SetProp(vm, "isMusic", true);
        VolumeSlider vs = root.AddComponent<VolumeSlider>();
        SetProp(vs, "mixer", mixer);
        SetProp(vs, "slider", sfx);
        SetProp(vs, "parameter", "SfxVolume");
        SetProp(vs, "isMusic", false);

        // ---- win screen
        GameObject win = OverlayScreen(t, "WinPanel", new Vector2(960f, 760f), out card);
        Label(card, "Title", "LEVEL COMPLETE", 80f, TextAlignmentOptions.Center, Palette.Pickup, MID, new Vector2(0f, 290f), new Vector2(900f, 110f));
        Label(card, "Detail", "", 42f, TextAlignmentOptions.Center, Palette.Text, MID, new Vector2(0f, 60f), new Vector2(860f, 280f));
        Btn(card, "NextButton", "Next level", MID, new Vector2(-300f, -250f), new Vector2(270f, 100f), 34f, nav.Next);
        Btn(card, "RestartButton", "Restart", MID, new Vector2(0f, -250f), new Vector2(270f, 100f), 34f, nav.Restart);
        Btn(card, "MenuButton", "Main menu", MID, new Vector2(300f, -250f), new Vector2(270f, 100f), 34f, nav.Menu);
        win.SetActive(false);

        // ---- game over screen
        GameObject lose = OverlayScreen(t, "LosePanel", new Vector2(900f, 700f), out card);
        Label(card, "Title", "GAME OVER", 90f, TextAlignmentOptions.Center, Palette.Hazard, MID, new Vector2(0f, 260f), new Vector2(840f, 120f));
        Label(card, "Detail", "", 42f, TextAlignmentOptions.Center, Palette.Text, MID, new Vector2(0f, 60f), new Vector2(840f, 220f));
        Btn(card, "RestartButton", "Restart", MID, new Vector2(-180f, -230f), new Vector2(320f, 100f), 34f, nav.Restart);
        Btn(card, "MenuButton", "Main menu", MID, new Vector2(180f, -230f), new Vector2(320f, 100f), 34f, nav.Menu);
        lose.SetActive(false);

        SavePrefab(root, "HUD");
    }

    // ------------------------------------------------------------------ the LevelLoader prefab (loading bar)
    static void BuildLoader()
    {
        GameObject root = new GameObject("LevelLoader", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        SetCanvas(root, 100);                            // above every other canvas
        LevelLoader loader = root.AddComponent<LevelLoader>();

        Image screen = Img(root.transform, "LoadingScreen", Palette.Panel, true);   // solid, and it blocks clicks while loading
        Stretch(screen.gameObject);
        TextMeshProUGUI label = Label(screen.transform, "Label", "Loading 0%", 56f, TextAlignmentOptions.Center, Palette.Text, MID, new Vector2(0f, 50f), new Vector2(800f, 90f));
        Image back = Img(screen.transform, "BarBack", ColourUtil.Hsv(Palette.BaseHue, 0.30f, 0.32f), false);
        Place(back.gameObject, MID, new Vector2(0f, -40f), new Vector2(800f, 40f));
        Image fill = Img(back.transform, "Bar", Palette.Pickup, false);
        Stretch(fill.gameObject);
        Fill(fill, true);

        SetProp(loader, "screen", screen.gameObject);
        SetProp(loader, "bar", fill);
        SetProp(loader, "label", label);
        screen.gameObject.SetActive(false);
        SavePrefab(root, "LevelLoader");
    }
}

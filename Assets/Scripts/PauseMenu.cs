using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Lecture 08 slides 33 to 36. Time.timeScale = 0 stops physics, deltaTime, WaitForSeconds and Normal-update
// Animators. It does NOT stop Update, input, the UI or audio, so:
//   * Escape can still read the key at timeScale 0 (that is why it can also resume);
//   * the player's Update must return early while IsPaused;
//   * AudioListener.pause stops the music;
//   * Resume() comes before every restart or quit, because timeScale and statics survive a scene load.
public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject firstButton;      // Resume: the button the keyboard starts on

    public static bool IsPaused { get; private set; }

    void Awake()
    {
        IsPaused = false;                          // a static from the last scene must not leak in
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void Update()
    {
        if (!EscapePressed()) return;
        if (IsPaused) Resume();
        else Pause();
    }

    bool CanPause
    {
        get { return GameManager.Instance == null || !GameManager.Instance.PanelOpen; }   // not over the win or game-over screen
    }

    public void Pause()
    {
        if (IsPaused || !CanPause) return;
        IsPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
        Select(firstButton);                       // so the arrows and Enter work the moment it opens
    }

    public void Resume()
    {
        IsPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    public void Restart()
    {
        Resume();                                  // first: otherwise the new scene starts frozen
        if (LevelLoader.Instance != null) LevelLoader.Instance.Reload();
    }

    public void QuitToMenu()
    {
        Resume();
        if (LevelLoader.Instance != null) LevelLoader.Instance.LoadMenu();
    }

    // Called by the LevelLoader on every load, so no pause state can leak into the next scene.
    public static void ForceResume() { IsPaused = false; }

    public static void Select(GameObject button)
    {
        if (EventSystem.current == null || button == null) return;
        EventSystem.current.SetSelectedGameObject(null);      // clear first, so selecting the same button again still highlights it
        EventSystem.current.SetSelectedGameObject(button);
    }

    static bool EscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Escape)) return true;
#endif
        return false;
    }
}

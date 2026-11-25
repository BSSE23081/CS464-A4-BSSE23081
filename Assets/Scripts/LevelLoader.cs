using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Lecture 10: EVERY scene change in the game goes through this one script.
//   LoadNext()  win screens      Reload() / LoadMenu()  pause and game-over screens
//   Load(n)     level select     LoadCredits()          main menu
// It is not DontDestroyOnLoad: like everything else in a scene it is destroyed and made again on every load.
// What survives a load is the music player, the SfxPlayer, PlayerPrefs, and (traps!) Time.timeScale and statics.
public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }

    [SerializeField] GameObject screen;     // the loading screen, off until a load starts
    [SerializeField] Image bar;             // Image Type: Filled, Horizontal
    [SerializeField] TMP_Text label;

    bool loading;

    void Awake()
    {
        Instance = this;
        if (screen != null) screen.SetActive(false);
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    public int CurrentIndex { get { return SceneManager.GetActiveScene().buildIndex; } }

    // Credits is the last scene, so the one after it wraps to 0, the main menu (Lecture 10, slide 24).
    public void LoadNext() { Load((CurrentIndex + 1) % SceneManager.sceneCountInBuildSettings); }
    public void Reload() { Load(CurrentIndex); }
    public void LoadMenu() { Load(0); }
    public void LoadCredits() { Load(SceneManager.sceneCountInBuildSettings - 1); }

    public void Load(int buildIndex)
    {
        if (loading) return;
        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError("LevelLoader: no scene at build index " + buildIndex + ". Check File > Build Profiles > Scene List (MainMenu 0, Level01..05 = 1..5, Credits 6).");
            return;
        }
        loading = true;
        Time.timeScale = 1f;            // timeScale, IsPaused and AudioListener.pause all survive a load...
        AudioListener.pause = false;
        PauseMenu.ForceResume();        // ...so every load puts them back first
        StartCoroutine(LoadRoutine(buildIndex));
    }

    IEnumerator LoadRoutine(int buildIndex)
    {
        if (screen != null) screen.SetActive(true);
        SetBar(0f);

        AsyncOperation op = SceneManager.LoadSceneAsync(buildIndex);
        op.allowSceneActivation = false;            // hold at 90% so the bar can be seen to finish
        while (op.progress < 0.9f)
        {
            SetBar(Mathf.Clamp01(op.progress / 0.9f));   // progress stops at 0.9 until activation
            yield return null;
        }
        SetBar(1f);
        yield return new WaitForSecondsRealtime(0.2f);
        op.allowSceneActivation = true;
    }

    void SetBar(float t)
    {
        if (bar != null) bar.fillAmount = t;
        if (label != null) label.text = "Loading " + Mathf.RoundToInt(t * 100f) + "%";
    }
}

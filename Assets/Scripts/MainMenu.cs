using UnityEngine;

// The title screen. Every button is wired in the Inspector (On Click), and every scene change goes through the LevelLoader.
public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject levelPanel;
    [SerializeField] GameObject playButton;       // selected when the main panel opens
    [SerializeField] LevelSelect levelSelect;

    void Start()
    {
        Time.timeScale = 1f;
        ShowMain();
    }

    public void ShowMain()
    {
        mainPanel.SetActive(true);
        levelPanel.SetActive(false);
        PauseMenu.Select(playButton);
    }

    public void ShowLevelSelect()
    {
        mainPanel.SetActive(false);
        levelPanel.SetActive(true);
        levelSelect.Refresh();
        PauseMenu.Select(levelSelect.FirstSelectable());
    }

    // Play continues at the highest level you have unlocked (Level 1 on a fresh save).
    public void Play() { LevelLoader.Instance.Load(SaveData.Unlocked); }

    public void Credits() { LevelLoader.Instance.LoadCredits(); }

    public void ResetProgress()
    {
        SaveData.ResetProgress();
        levelSelect.Refresh();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;   // Application.Quit does nothing in the Editor
#else
        Application.Quit();
#endif
    }
}

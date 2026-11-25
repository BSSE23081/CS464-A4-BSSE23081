using UnityEngine;

// The credits scene: a list of every sound and track, then back to the menu through the LevelLoader.
public class CreditsScreen : MonoBehaviour
{
    [SerializeField] GameObject backButton;

    void Start()
    {
        Time.timeScale = 1f;
        PauseMenu.Select(backButton);
    }

    public void Back() { LevelLoader.Instance.LoadMenu(); }
}

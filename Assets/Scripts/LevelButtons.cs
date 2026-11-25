using UnityEngine;

// The win and game-over buttons live in the HUD prefab, so they cannot point at a scene object.
// They point here instead, and these three methods hand the job to the LevelLoader.
public class LevelButtons : MonoBehaviour
{
    public void Next() { if (LevelLoader.Instance != null) LevelLoader.Instance.LoadNext(); }
    public void Restart() { if (LevelLoader.Instance != null) LevelLoader.Instance.Reload(); }
    public void Menu() { if (LevelLoader.Instance != null) LevelLoader.Instance.LoadMenu(); }
}

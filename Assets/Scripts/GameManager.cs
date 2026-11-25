using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Objective { CollectCoins, ReachGoal, Survive }

// One GameManager per level: it owns the score, the lives, the objective, and the win and lose states.
// Lecture 06's GameManager (score, win, restart) plus Lecture 07's lives, respawn and Lose().
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Objective")]
    public Objective objective = Objective.CollectCoins;
    public int coinsRequired = 8;        // CollectCoins: win at this score. ReachGoal / Survive: needed as well.
    public float surviveSeconds = 0f;    // Survive: the clock to outlast
    public float timeLimit = 0f;         // > 0: reach the goal before it runs out, or lose
    public string levelTitle = "Level";
    [TextArea] public string intro = "";

    [Header("Lives")]
    public int lives = 3;
    public float respawnDelay = 0.6f;
    public float spawnProtection = 1.5f;

    [Header("Scene references (set by the level builder)")]
    public Health player;
    public Transform respawnPoint;
    public TMP_Text scoreText;
    public TMP_Text livesText;
    public TMP_Text objectiveText;
    public TMP_Text timerText;
    public TMP_Text messageText;
    public GameObject winPanel;
    public GameObject losePanel;
    public TMP_Text winDetail;
    public TMP_Text loseDetail;
    public GameObject nextButton;
    public TMP_Text nextButtonLabel;
    public GameObject winFirstButton;    // selected when the win screen opens, so Enter works at once
    public GameObject loseFirstButton;   // selected when the game-over screen opens

    enum State { Playing, Won, Lost }
    State state = State.Playing;
    public bool PanelOpen => state != State.Playing;
    public bool Won => state == State.Won;
    int score;
    int totalCoins;
    float elapsed;
    bool respawning;
    int lastShownSecond = -1;
    Coroutine messageRoutine;

    void Awake()
    {
        Instance = this;      // Lecture 06 slide 29: set in Awake, so every Start can use it
        Time.timeScale = 1f;  // a level always starts running, even after a frozen win or loss
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex < 0)
            Debug.LogWarning("This scene is not in the Build Profiles scene list. Restart and Next Level still work in the editor, but add every level to the list.");
        totalCoins = FindObjectsByType<Coin>(FindObjectsSortMode.None).Length;
        if (coinsRequired > totalCoins)
            Debug.LogWarning($"{levelTitle}: coinsRequired ({coinsRequired}) is higher than the coins in the level ({totalCoins}). It can't be won.");

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        UpdateUI();
        if (!string.IsNullOrEmpty(intro)) ShowMessage(intro, 6f);
    }

    void Update()
    {
        if (state != State.Playing) return;
        elapsed += Time.deltaTime;

        if (timeLimit > 0f && elapsed >= timeLimit)
        {
            Lose("Time's up!");
            return;
        }
        if (objective == Objective.Survive) CheckWin();
        UpdateTimerText();
    }

    // ---------------------------------------------------------------- score and win
    public void AddScore(int amount)
    {
        if (state != State.Playing) return;
        score += amount;
        UpdateUI();
        CheckWin();
    }

    void CheckWin()
    {
        if (state != State.Playing) return;
        switch (objective)
        {
            case Objective.CollectCoins:
                if (score >= coinsRequired) Win();
                break;
            case Objective.Survive:
                if (elapsed >= surviveSeconds && score >= coinsRequired) Win();
                break;
            // ReachGoal is decided by the Goal, in ReachGoal() below.
        }
    }

    public void ReachGoal()
    {
        if (state != State.Playing || objective != Objective.ReachGoal) return;
        if (score >= coinsRequired) Win();
        else ShowMessage($"The goal needs {coinsRequired - score} more coin(s)", 2f);
    }

    // Lecture 10: a win records the time, opens the next level, and only then shows the screen.
    void Win()
    {
        state = State.Won;
        int level = SceneManager.GetActiveScene().buildIndex;       // Level01 to Level05 are build indices 1 to 5
        bool best = SaveData.Record(level, elapsed);                // true only if faster than the saved time
        SaveData.Unlock(level + 1);                                 // one more level opens
        string bestLine = best ? "NEW BEST TIME!" : (SaveData.HasBest(level) ? "Best: " + SaveData.FormatTime(SaveData.GetBest(level)) : "");
        string detail = $"Coins: {score} / {totalCoins}\nTime: {FormatTime(elapsed)}\nLives left: {lives}\n{bestLine}";
        SetText(winDetail, detail);
        if (nextButtonLabel != null) nextButtonLabel.text = level >= SaveData.LevelCount ? "Credits" : "Next level";
        if (winPanel != null) winPanel.SetActive(true);
        HideMessage();
        SfxPlayer.Play(Sfx.Win);
        Time.timeScale = 0f;
        PauseMenu.Select(winFirstButton);
    }

    // ---------------------------------------------------------------- lives, respawn, lose
    // Health calls this at <= 0 hearts; a KillZone calls it when you fall.
    public void PlayerDied()
    {
        if (state != State.Playing || respawning) return;
        lives--;
        UpdateUI();
        if (lives <= 0) Lose("You ran out of lives");
        else StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        respawning = true;
        yield return new WaitForSeconds(respawnDelay);   // a beat to see it happen; also lets a queued knockback finish
        if (state == State.Playing)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            pc.Respawn(respawnPoint.position);           // standing still, at the respawn point
            player.Restore(spawnProtection);             // full hearts, and a moment of protection
        }
        respawning = false;
        UpdateUI();
    }

    void Lose(string reason)
    {
        state = State.Lost;
        SetText(loseDetail, reason + $"\nCoins: {score} / {totalCoins}");
        if (losePanel != null) losePanel.SetActive(true);
        HideMessage();
        SfxPlayer.Play(Sfx.Lose);
        Time.timeScale = 0f;
        PauseMenu.Select(loseFirstButton);
    }

    public void SetCheckpoint(Transform point)
    {
        if (respawnPoint == point) return;
        respawnPoint = point;
        ShowMessage("Checkpoint!", 1.5f);
    }

    // ---------------------------------------------------------------- restart, next level, menu
    // The buttons call these; the LevelLoader does the loading (and puts Time.timeScale back to 1 first).
    public void Restart() { if (LevelLoader.Instance != null) LevelLoader.Instance.Reload(); }
    public void NextLevel() { if (LevelLoader.Instance != null) LevelLoader.Instance.LoadNext(); }
    public void MainMenu() { if (LevelLoader.Instance != null) LevelLoader.Instance.LoadMenu(); }

    // ---------------------------------------------------------------- UI (text is set only when it changes)
    public void UpdateUI()
    {
        string coinLabel = objective == Objective.CollectCoins ? "Coins" : "Coins (needed)";
        SetText(scoreText, $"{coinLabel}: {score} / {coinsRequired}");

        SetText(livesText, $"Lives: {lives}");

        switch (objective)
        {
            case Objective.CollectCoins:
                SetText(objectiveText, $"{levelTitle}: collect {coinsRequired} coins");
                break;
            case Objective.ReachGoal:
                SetText(objectiveText, $"{levelTitle}: reach the goal with {coinsRequired} coins");
                break;
            case Objective.Survive:
                SetText(objectiveText, $"{levelTitle}: collect {coinsRequired} coins and survive {surviveSeconds:0}s");
                break;
        }
        UpdateTimerText();
    }

    void UpdateTimerText()
    {
        if (timerText == null) return;
        if (timeLimit > 0f) SetSecond("Time", timeLimit - elapsed);
        else if (objective == Objective.Survive) SetSecond("Survive", surviveSeconds - elapsed);
        else if (timerText.transform.parent.gameObject.activeSelf) timerText.transform.parent.gameObject.SetActive(false);
    }

    void SetSecond(string label, float secondsLeft)
    {
        int shown = Mathf.Max(0, Mathf.CeilToInt(secondsLeft));
        if (shown == lastShownSecond) return;      // only when the shown second changes
        lastShownSecond = shown;
        timerText.transform.parent.gameObject.SetActive(true);   // the text sits on its own solid panel: show the panel
        timerText.text = $"{label} {shown / 60}:{shown % 60:00}";
    }

    static void SetText(TMP_Text t, string s)
    {
        if (t != null && t.text != s) t.text = s;
    }

    public void ShowMessage(string message, float seconds = 2.5f)
    {
        if (messageText == null || state != State.Playing) return;
        messageText.text = message;
        messageText.transform.parent.gameObject.SetActive(true);
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(HideMessageAfter(seconds));
    }

    IEnumerator HideMessageAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        messageText.transform.parent.gameObject.SetActive(false);
        messageRoutine = null;
    }

    void HideMessage()
    {
        if (messageText != null) messageText.transform.parent.gameObject.SetActive(false);
    }

    static string FormatTime(float seconds)
    {
        int s = Mathf.FloorToInt(seconds);
        return $"{s / 60}:{s % 60:00}";
    }
}

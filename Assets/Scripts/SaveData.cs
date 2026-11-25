using UnityEngine;

// The only script that spells a PlayerPrefs key (Lecture 10). Every read has a default, so a fresh install
// (or Edit > Clear All PlayerPrefs) works: only Level 1 is open and the volumes are at 1.
public static class SaveData
{
    public const int LevelCount = 5;

    const string UnlockedKey = "unlocked";      // int: the highest level you may play. Default 1.
    const string BestPrefix = "best";           // float: best1 ... best5, seconds. Check HasKey first.
    const string MusicKey = "MusicVolume";      // float 0.0001 to 1. Default 1.
    const string SfxKey = "SfxVolume";          // float 0.0001 to 1. Default 1.

    // ---- unlocked levels
    public static int Unlocked { get { return Mathf.Clamp(PlayerPrefs.GetInt(UnlockedKey, 1), 1, LevelCount); } }

    public static bool IsUnlocked(int level) { return level <= Unlocked; }

    // Only ever goes up. Unlock(6) after the last level is clamped to 5.
    public static void Unlock(int level)
    {
        level = Mathf.Clamp(level, 1, LevelCount);
        if (level <= Unlocked) return;
        PlayerPrefs.SetInt(UnlockedKey, level);
        PlayerPrefs.Save();
    }

    // ---- best times
    static string BestKey(int level) { return BestPrefix + level; }

    public static bool HasBest(int level) { return PlayerPrefs.HasKey(BestKey(level)); }

    // 0 when there is no time yet: ask HasBest first.
    public static float GetBest(int level) { return PlayerPrefs.GetFloat(BestKey(level), 0f); }

    // Saves only when it is faster. True means "a new best" (the first win of a level counts).
    public static bool Record(int level, float seconds)
    {
        if (level < 1 || level > LevelCount) return false;
        if (HasBest(level) && seconds >= GetBest(level)) return false;
        PlayerPrefs.SetFloat(BestKey(level), seconds);
        PlayerPrefs.Save();
        return true;
    }

    // ---- volumes (the slider values, 0.0001 to 1; the mixer gets decibels)
    public static float MusicVolume
    {
        get { return PlayerPrefs.GetFloat(MusicKey, 1f); }
        set { PlayerPrefs.SetFloat(MusicKey, value); PlayerPrefs.Save(); }
    }

    public static float SfxVolume
    {
        get { return PlayerPrefs.GetFloat(SfxKey, 1f); }
        set { PlayerPrefs.SetFloat(SfxKey, value); PlayerPrefs.Save(); }
    }

    // Progress only: the volumes are a setting, not progress.
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(UnlockedKey);
        for (int i = 1; i <= LevelCount; i++) PlayerPrefs.DeleteKey(BestKey(i));
        PlayerPrefs.Save();
    }

    public static string FormatTime(float seconds)
    {
        int s = Mathf.FloorToInt(seconds);
        return (s / 60) + ":" + (s % 60).ToString("00");
    }
}

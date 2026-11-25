using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// The menu command and the six scenes: Level01 to Level05 and the Lecture 07 arena.
// Each level is a layout (Floor, Wall), things to collect (Coin), dangers (Spikes, Crusher, Sweeper, Guard)
// and one Config. Change a number, then run the menu command again to rebuild.
public static partial class CS464Builder
{
    [MenuItem("CS464/Build Prefabs, Levels and Build Settings")]
    public static void BuildAll()
    {
        if (TMP_Settings.instance == null)
        {
            EditorUtility.DisplayDialog("CS464",
                "TextMesh Pro needs its essential resources first.\n\nWindow > TextMeshPro > Import TMP Essential Resources, then run this menu command again.", "OK");
            return;
        }
        if (!EditorUtility.DisplayDialog("CS464",
                "This (re)creates Assets/Prefabs, Assets/Materials and the scenes MainMenu, Level01 to Level05 and Credits in Assets/Scenes, " +
                "and sets the Build Profiles scene list.\n\nCommit first: git can bring anything back.",
                "Build", "Cancel"))
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        BuildPrefabs();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        LoadPrefabs();

        BuildMainMenuScene();
        Level01();
        Level02();
        Level03();
        Level04();
        Level05();
        BuildCreditsScene();

        // The play order IS the build index order: MainMenu 0, Level01 to Level05 = 1 to 5, Credits 6.
        string[] names = { "MainMenu", "Level01", "Level02", "Level03", "Level04", "Level05", "Credits" };
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[names.Length];
        for (int i = 0; i < names.Length; i++)
            scenes[i] = new EditorBuildSettingsScene(SceneDir + "/" + names[i] + ".unity", true);
        EditorBuildSettings.scenes = scenes;

        PaletteReport();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(SceneDir + "/MainMenu.unity");
        EditorUtility.DisplayDialog("CS464", "Done. MainMenu, Level01 to Level05 and Credits are built and in the Build Profiles scene list.\n\nOpen MainMenu and press Play. The Console has the palette's measured contrast ratios.", "OK");
    }

    [MenuItem("CS464/Print Palette Report")]
    public static void PaletteReport()
    {
        string r = "CS464 palette (base hue " + Palette.BaseHue + ", triadic: " + Palette.HazardHue + " and " + Palette.PickupHue + ")\n";
        r += Row("HUD text on panel", Palette.Text, Palette.Panel);
        r += Row("Score (pickup green) on panel", Palette.Pickup, Palette.Panel);
        r += Row("Button label (panel) on button", Palette.Panel, Palette.Button);
        r += Row("Button label on highlighted", Palette.Panel, Palette.ButtonHighlighted);
        r += Row("Button label on selected", Palette.Panel, Palette.ButtonSelected);
        r += Row("Button label on pressed", Palette.Panel, Palette.ButtonPressed);
        r += Row("Game-over title (hazard) on panel [large text, 3:1]", Palette.Hazard, Palette.Panel);
        r += "Value tiers (relative luminance): panel " + ColourUtil.RelativeLuminance(Palette.Panel).ToString("0.000") +
             ", hazard " + ColourUtil.RelativeLuminance(Palette.Hazard).ToString("0.000") +
             ", pickup " + ColourUtil.RelativeLuminance(Palette.Pickup).ToString("0.000") +
             ", player " + ColourUtil.RelativeLuminance(Palette.Player).ToString("0.000") + "\n";
        r += Row("player vs pickup", Palette.Player, Palette.Pickup);
        r += Row("pickup vs hazard", Palette.Pickup, Palette.Hazard);
        r += Row("hazard vs a floor (hue 215)", Palette.Hazard, Palette.Floor(215f));
        r += Row("player vs a floor (hue 215)", Palette.Player, Palette.Floor(215f));
        Debug.Log(r);
    }

    static string Row(string what, Color a, Color b)
    {
        return what.PadRight(58) + ColourUtil.Contrast(a, b).ToString("0.00") + " : 1\n";
    }

    // =====================================================================================
    // LEVEL 1 - First Steps.  New idea: coins, and the bounce pad. Nothing hurts.
    // Objective: collect all 8 coins.  Lives: 5.
    // A square courtyard with a raised ledge you can only reach by bouncing. The only way to lose is to fall off.
    // =====================================================================================
    static void Level01()
    {
        BeginScene(new Color(0.30f, 0.58f, 0.56f), new Color(0.16f, 0.34f, 0.36f));
        Floor(-10, 10, -10, 10);
        Wall(-11, -10, -11, 10);
        Wall(10, 11, -11, 10);
        Wall(-10, 10, -11, -10);
        Floor(-4, 4, 12, 18, 3f);              // the ledge, 3 m up
        Bounce(0, 7);                          // the bouncy pad: VelocityChange, 9 m/s up

        Coin(-6, -4); Coin(6, -4); Coin(0, 0); Coin(-5, 4); Coin(5, 4);
        Coin(-2, 15, 3f); Coin(0, 15, 3f); Coin(2, 15, 3f);

        StartAt(0, -7);
        Finish("Level01", new Config
        {
            title = "Level 1",
            intro = "Level 1 - First Steps\nCollect all 8 coins. Space jumps. Run onto the bouncy pad and keep going forward to reach the high ledge.",
            objective = Objective.CollectCoins,
            coinsRequired = 8,
            lives = 5
        }, 0f, 4f);
    }

    // =====================================================================================
    // LEVEL 2 - Beat the Clock.  New idea: spikes, a heart at a time, and a timer.
    // Objective: reach the goal with 4 coins in 50 s.  Lives: 4.  A checkpoint halfway.
    // A winding path of platforms over gaps. Each spike strip has a safe lane, and one coin sits on a risky detour.
    // =====================================================================================
    static void Level02()
    {
        BeginScene(new Color(0.34f, 0.46f, 0.66f), new Color(0.20f, 0.28f, 0.45f));
        Floor(-4, 4, -4, 4);          // start
        Floor(-3, 3, 4, 14);          // first strip
        Floor(-4, 4, 17, 27);         // second platform, 3 m gap before it
        Floor(7, 17, 17, 27);         // third, 3 m gap to the east
        Floor(8, 16, 30, 40);         // the goal platform, 3 m gap to the north

        Spikes(-3, 0, 8, 9.5f);                      // the safe lane is x 0 to 3
        Spikes(-4, -1, 21, 22.5f);                   // the safe lane is the middle, 2 m wide
        Spikes(1, 4, 21, 22.5f);
        Spikes(9, 13, 21, 23);
        Spikes(8, 12, 33, 35);

        Coin(0, 6); Coin(-1.5f, 8.75f);              // the second one is on the spikes: a risky detour
        Coin(-3, 19); Coin(0, 23);
        Coin(10, 18.5f); Coin(15, 25); Coin(14, 34);

        CheckpointAt(0, 19.5f);
        GoalAt(12, 38);
        StartAt(0, 0);
        Finish("Level02", new Config
        {
            title = "Level 2",
            intro = "Level 2 - Beat the Clock\nGrab 4 coins and reach the glowing goal before the timer ends. Spikes cost a heart: use the safe lanes.",
            objective = Objective.ReachGoal,
            coinsRequired = 4,
            timeLimit = 50f,
            lives = 4
        }, 8f, 20f);
    }

    // =====================================================================================
    // LEVEL 3 - Hunted.  New idea: guards that patrol, chase and give up. Spikes beside their routes.
    // Objective: collect all 10 coins.  Lives: 3.
    // A walled arena with a pit in the middle. One guard circles the pit (loop), one ping-pongs along the north wall.
    // =====================================================================================
    static void Level03()
    {
        BeginScene(new Color(0.46f, 0.55f, 0.32f), new Color(0.26f, 0.32f, 0.18f));
        Floor(-16, 16, 0, 11);
        Floor(-16, 16, 21, 32);
        Floor(-16, -5, 11, 21);
        Floor(5, 16, 11, 21);
        Wall(-17, -16, -1, 33);
        Wall(16, 17, -1, 33);
        Wall(-16, 16, -1, 0);
        Wall(-16, 16, 32, 33);

        Spikes(-11.5f, -9, 13, 19);
        Spikes(9, 11.5f, 13, 19);

        Coin(-13, 2); Coin(13, 2); Coin(0, 6); Coin(7, 9);
        Coin(-13, 29); Coin(13, 29); Coin(0, 27); Coin(-7, 23);
        Coin(-10.25f, 16);        // on the spikes: a risky detour
        Coin(13, 16);

        // Guard A circles the pit; chaseRange 6.5 m, giveUpRange 10 m, chases at 3.3 m/s (you run at 6.5).
        Guard("GuardA", PatrolMode.Loop, 2f, 3.3f, 6.5f, 10f, true, 0f, 0f,
            new Vector2(-8, 9), new Vector2(8, 9), new Vector2(8, 23), new Vector2(-8, 23));
        // Guard B ping-pongs along the north wall.
        Guard("GuardB", PatrolMode.PingPong, 2.2f, 3.5f, 6f, 9f, true, 0f, 0f,
            new Vector2(-12, 27), new Vector2(0, 27), new Vector2(12, 27));

        StartAt(-12, 2);
        Finish("Level03", new Config
        {
            title = "Level 3",
            intro = "Level 3 - Hunted\nCollect all 10 coins. Guards chase when you get close and give up when you get far. Mind the pit and the spikes.",
            objective = Objective.CollectCoins,
            coinsRequired = 10,
            lives = 3
        }, 0f, 16f);
    }

    // =====================================================================================
    // LEVEL 4 - Crusher Gauntlet.  New idea: the crusher (it warns first). A guard beside it.
    // Objective: reach the goal with 5 coins.  Lives: 3.  A checkpoint after the second crusher.
    // A long narrow corridor with a wide chamber in the middle, then a jump to the goal.
    // =====================================================================================
    static void Level04()
    {
        BeginScene(new Color(0.48f, 0.40f, 0.62f), new Color(0.28f, 0.22f, 0.40f));
        Floor(-4, 4, -5, 5);
        Floor(-2.5f, 2.5f, 5, 16);
        Floor(-8, 8, 16, 24);             // the chamber
        Floor(-2.5f, 2.5f, 24, 36);
        Floor(-4, 4, 39, 50);             // 3 m gap, then the goal platform

        // Both crushers fill the corridor. rest 2.4 s, warn 0.7 s, strike 0.4 s, 2 damage.
        Crusher(0, 12, 5, 3, 2.4f, 0.7f, 0.4f, 0f);
        Crusher(0, 29, 5, 3, 2.4f, 0.7f, 0.4f, 1.5f);

        // The guard beside the first crusher walks the chamber and chases you down the corridor.
        Guard("ChamberGuard", PatrolMode.Loop, 2.2f, 3.5f, 6f, 9f, true, 0f, 0f,
            new Vector2(-6, 17), new Vector2(6, 17), new Vector2(6, 23), new Vector2(-6, 23));

        Coin(0, 8); Coin(0, 15); Coin(-6.5f, 23); Coin(6.5f, 17); Coin(0, 26); Coin(0, 33); Coin(0, 42);

        CheckpointAt(0, 33);
        GoalAt(0, 45);
        StartAt(0, 0);
        Finish("Level04", new Config
        {
            title = "Level 4",
            intro = "Level 4 - Crusher Gauntlet\nA crusher flashes before it strikes: wait, then run. Collect 5 coins and reach the goal.",
            objective = Objective.ReachGoal,
            coinsRequired = 5,
            lives = 3
        }, 0f, 22f);
    }

    // =====================================================================================
    // LEVEL 5 - Last Stand.  New idea: two speed pickups, reachable within 5 s of each other. Everything so far.
    // Objective: collect 8 coins AND survive 60 s.  Lives: 3.
    // A cross-shaped platform with no walls: a chasing guard circles the middle, ping-pong guards patrol two
    // arms, a sweeper crosses the north arm, crushers guard the south arm and the middle, and the edges are open.
    // =====================================================================================
    static void Level05()
    {
        BeginScene(new Color(0.55f, 0.36f, 0.36f), new Color(0.34f, 0.20f, 0.20f));
        Floor(-8, 8, -8, 8);              // the middle
        Floor(-4, 4, 8, 22);              // north arm
        Floor(-4, 4, -22, -8);            // south arm
        Floor(8, 22, -4, 4);              // east arm
        Floor(-22, -8, -4, 4);            // west arm

        Crusher(0, -11, 8, 2.5f, 2.2f, 0.8f, 0.4f, 0f);   // fills the south arm: you must time it
        Crusher(0, 0, 4, 4, 2.4f, 0.7f, 0.4f, 1f);        // the middle: a coin on it
        Sweeper(0, 14, 2.4f, 1f, Vector3.right, 5.6f, 3f);

        Spikes(-4, -1.5f, 16.5f, 18.5f);
        Spikes(15, 16.5f, -4, -1.5f);
        Spikes(-16.5f, -15, 1.5f, 4);

        Coin(0, 0);
        Coin(5.5f, 5.5f); Coin(-5.5f, 5.5f); Coin(5.5f, -5.5f); Coin(-5.5f, -5.5f);
        Coin(0, 11); Coin(0, 19.5f);
        Coin(12, 0); Coin(19.5f, 0);
        Coin(-12, 0); Coin(-19.5f, 0);
        Coin(0, -15);

        // Two speed pickups about 10 m apart: the second can be taken while the first is still running.
        Boost(-4, 3);
        Boost(4, -3);

        // A chaser round the middle, and one ping-pong guard on each side arm.
        Guard("RingGuard", PatrolMode.Loop, 2.5f, 3.5f, 7f, 10f, true, 0f, 0f,
            new Vector2(-6.5f, -6.5f), new Vector2(6.5f, -6.5f), new Vector2(6.5f, 6.5f), new Vector2(-6.5f, 6.5f));
        Guard("EastGuard", PatrolMode.PingPong, 2.5f, 3.5f, 5f, 8f, true, 0f, 0f,
            new Vector2(10, 0), new Vector2(15, 0), new Vector2(20, 0));
        Guard("WestGuard", PatrolMode.PingPong, 2.5f, 3.5f, 5f, 8f, true, 0f, 0f,
            new Vector2(-10, 0), new Vector2(-15, 0), new Vector2(-20, 0));

        StartAt(0, -20);
        Finish("Level05", new Config
        {
            title = "Level 5",
            intro = "Level 5 - Last Stand\nCollect 8 coins and survive 60 seconds. Orbs with a bolt give a speed boost: grab a second one to restart the clock.",
            objective = Objective.Survive,
            coinsRequired = 8,
            surviveSeconds = 60f,
            lives = 3
        }, 0f, 0f);
    }
}

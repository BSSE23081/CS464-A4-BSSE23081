using TMPro;
using UnityEditor;
using UnityEngine;

// Builds the materials and prefabs: Player, Coin, Guard, Spikes, Crusher, Sweeper, BouncePad,
// Goal, Checkpoint, PowerUps, KillZone, GameManager and the HUD. Run from the CS464 menu.
public static partial class CS464Builder
{
    const string PrefabDir = "Assets/Prefabs";
    const string SceneDir = "Assets/Scenes";
    const string MatDir = "Assets/Materials";

    // ------------------------------------------------------------------ small helpers
    static void EnsureFolder(string parent, string child)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + child)) AssetDatabase.CreateFolder(parent, child);
    }

    static Material M(string name, Color c, float emission = 0f)
    {
        string path = MatDir + "/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            Shader sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) sh = Shader.Find("Standard");
            m = new Material(sh);
            AssetDatabase.CreateAsset(m, path);
        }
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        if (emission > 0f && m.HasProperty("_EmissionColor"))
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", c * emission);
        }
        EditorUtility.SetDirty(m);
        return m;
    }

    // Walls are slippery so the ball slides along them instead of sticking; floors keep their friction so it rolls.
    static PhysicsMaterial WallSlippery()
    {
        string path = MatDir + "/WallSlippery.asset";
        PhysicsMaterial pm = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);
        if (pm == null)
        {
            pm = new PhysicsMaterial("WallSlippery");
            AssetDatabase.CreateAsset(pm, path);
        }
        pm.dynamicFriction = 0f;
        pm.staticFriction = 0f;
        pm.bounciness = 0f;
        pm.frictionCombine = PhysicsMaterialCombine.Minimum;
        EditorUtility.SetDirty(pm);
        return pm;
    }

    static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, bool keepCollider)
    {
        GameObject g = GameObject.CreatePrimitive(type);
        g.name = name;
        if (parent != null) g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = mat;
        if (!keepCollider)
        {
            Collider c = g.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);
        }
        return g;
    }

    static GameObject SavePrefab(GameObject go, string name)
    {
        string path = PrefabDir + "/" + name + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static T Trigger<T>(GameObject go) where T : Collider
    {
        T c = go.AddComponent<T>();
        c.isTrigger = true;
        return c;
    }

    // ------------------------------------------------------------------ the prefabs
    static void BuildPrefabs()
    {
        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets", "Materials");
        EnsureAudio();           // the mixer and the clips first: prefabs point at them
        BuildEffects();          // PickupBurst and PowerUpBurst, Stop Action = Destroy
        BuildAudioPrefabs();     // SfxPlayer and PersistentMusic
        BuildLoader();           // LevelLoader with its loading bar
        ParticleSystem pickupFx = Prefab<ParticleSystem>("PickupBurst");
        ParticleSystem powerFx = Prefab<ParticleSystem>("PowerUpBurst");

        // Every colour comes from Palette: indigo base, orange-red for danger, green for pickups, split by VALUE.
        Material player = M("Player", Palette.Player);
        Material coin = M("Coin", Palette.Pickup, 0.15f);
        Material guard = M("Guard", Palette.Hazard);
        Material guardEye = M("GuardEye", Palette.Text);
        Material spikeBase = M("SpikeBase", Palette.Panel);
        Material crusherBase = M("CrusherBase", ColourUtil.Hsv(Palette.BaseHue, 0.30f, 0.22f));
        Material warning = M("Warning", Palette.Text, 0.6f);
        Material slab = M("CrusherSlab", Palette.Hazard);
        Material sweeper = M("Sweeper", Palette.Hazard);
        Material bounce = M("BouncePad", Palette.Button, 0.2f);
        Material goal = M("Goal", Palette.Pickup, 0.3f);
        Material pole = M("CheckpointPole", Palette.Text);
        Material flag = M("CheckpointFlag", Palette.Button, 0.2f);
        Material speedMat = M("PowerSpeed", Palette.Pickup, 0.3f);
        Material shieldMat = M("PowerShield", Palette.Button, 0.3f);

        // Player: tagged Player, a non-kinematic Rigidbody (what makes triggers fire), controller and Health.
        {
            GameObject go = Prim(PrimitiveType.Sphere, "Player", null, Vector3.zero, Vector3.one, player, true);
            go.tag = "Player";
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;   // a free-rolling ball: rotation is not frozen
            go.AddComponent<PlayerController>();
            go.AddComponent<Health>();
            // Two of the five Health listeners live here: a hit you can hear, and a hit you can see.
            go.AddComponent<DamageSfx>();
            HitSparks hs = go.AddComponent<HitSparks>();
            ParticleSystem sparks = MakeParticles("HitSparks", go.transform, Palette.Text, 22, 0.5f, 5f, 0.2f, false, false);
            SetProp(hs, "sparks", sparks);
            SavePrefab(go, "Player");
        }

        // Coin: a trigger with Coin.cs.
        {
            GameObject go = new GameObject("Coin");
            SphereCollider sc = Trigger<SphereCollider>(go);
            sc.radius = 0.6f;
            Coin cn = go.AddComponent<Coin>();
            SetProp(cn, "pickupEffect", pickupFx);
            GameObject vis = Prim(PrimitiveType.Cylinder, "Visual", go.transform, Vector3.zero, new Vector3(0.7f, 0.06f, 0.7f), coin, false);
            vis.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);   // a disc standing on its edge, so the spin shows
            SavePrefab(go, "Coin");
        }

        // Guard: kinematic Rigidbody, a trigger capsule, Hazard.cs and EnemyGuard.cs.
        {
            GameObject go = new GameObject("Guard");
            CapsuleCollider cc = Trigger<CapsuleCollider>(go);
            cc.radius = 0.55f;
            cc.height = 2f;
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            Hazard h = go.AddComponent<Hazard>();
            h.damage = 1;
            h.knockback = 8f;
            go.AddComponent<EnemyGuard>();
            // 3D sound 1: a hum you can hear coming. Logarithmic (-6 dB per doubling), full volume inside 2 m, cut at 16 m
            // (the guard notices you at 5 to 7 m and gives up at 8 to 10 m, so you hear it before it hears you).
            AddWorldSound(go, "loop_guard_hum", 0, 2f, 16f, 0.8f);
            Prim(PrimitiveType.Capsule, "Body", go.transform, Vector3.zero, new Vector3(1.1f, 1f, 1.1f), guard, false);
            Prim(PrimitiveType.Cube, "Eyes", go.transform, new Vector3(0f, 0.45f, 0.5f), new Vector3(0.7f, 0.18f, 0.2f), guardEye, false);
            SavePrefab(go, "Guard");
        }

        // Spikes: a trigger box and Hazard.cs. The level builder resizes it and adds the teeth.
        {
            GameObject go = new GameObject("Spikes");
            BoxCollider bc = Trigger<BoxCollider>(go);
            bc.size = new Vector3(1f, 0.8f, 1f);
            bc.center = new Vector3(0f, 0.4f, 0f);
            Hazard h = go.AddComponent<Hazard>();
            h.damage = 1;
            Prim(PrimitiveType.Cube, "Base", go.transform, new Vector3(0f, 0.075f, 0f), new Vector3(1f, 0.15f, 1f), spikeBase, false);
            SavePrefab(go, "Spikes");
        }

        // Crusher: off for restTime, warning for warnTime (0.7 s), then a strike for 2 damage.
        {
            GameObject go = new GameObject("Crusher");
            BoxCollider bc = Trigger<BoxCollider>(go);
            bc.size = new Vector3(4f, 1.4f, 3f);
            bc.center = new Vector3(0f, 0.7f, 0f);
            Hazard h = go.AddComponent<Hazard>();
            h.damage = 2;
            h.knockback = 10f;
            h.restTime = 2.4f;
            h.warnTime = 0.7f;
            h.strikeTime = 0.4f;
            AddWorldSound(go, "loop_hazard_buzz", 0, 2f, 12f, 0.6f);   // 3D sound 3: you hear a crusher before you see its warning
            Prim(PrimitiveType.Cube, "Base", go.transform, new Vector3(0f, 0.06f, 0f), new Vector3(4f, 0.12f, 3f), crusherBase, false);
            GameObject w = Prim(PrimitiveType.Cube, "Warning", go.transform, new Vector3(0f, 0.09f, 0f), new Vector3(3.8f, 0.14f, 2.8f), warning, false);
            GameObject s = Prim(PrimitiveType.Cube, "Slab", go.transform, new Vector3(0f, 0.7f, 0f), new Vector3(4f, 1.4f, 3f), slab, false);
            h.warning = w;
            h.slab = s;
            w.SetActive(false);
            s.SetActive(false);
            SavePrefab(go, "Crusher");
        }

        // Sweeper: a moving hazard on a kinematic Rigidbody.
        {
            GameObject go = new GameObject("Sweeper");
            BoxCollider bc = Trigger<BoxCollider>(go);
            bc.size = new Vector3(2.4f, 1.2f, 1f);
            bc.center = new Vector3(0f, 0.6f, 0f);
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            Hazard h = go.AddComponent<Hazard>();
            h.damage = 1;
            go.AddComponent<MovingHazard>();
            Prim(PrimitiveType.Cube, "Visual", go.transform, new Vector3(0f, 0.6f, 0f), new Vector3(2.4f, 1.2f, 1f), sweeper, false);
            SavePrefab(go, "Sweeper");
        }

        // Bounce pad (Part A's force: VelocityChange, once).
        {
            GameObject go = new GameObject("BouncePad");
            BoxCollider bc = Trigger<BoxCollider>(go);
            bc.size = new Vector3(3f, 0.6f, 3f);
            bc.center = new Vector3(0f, 0.3f, 0f);
            go.AddComponent<BouncePad>();
            Prim(PrimitiveType.Cylinder, "Pad", go.transform, new Vector3(0f, 0.08f, 0f), new Vector3(2.8f, 0.08f, 2.8f), bounce, false);
            SavePrefab(go, "BouncePad");
        }

        // Goal
        {
            GameObject go = new GameObject("Goal");
            BoxCollider bc = Trigger<BoxCollider>(go);
            bc.size = new Vector3(3f, 3f, 3f);
            bc.center = new Vector3(0f, 1.5f, 0f);
            go.AddComponent<Goal>();
            // 3D sound 2: a chime you can follow across the level. Linear: it falls in a straight line to silence at 35 m,
            // so the goal is heard from about half-way along a 30 m route but never from the next room's far end.
            AddWorldSound(go, "loop_goal_chime", 1, 3f, 35f, 0.7f);
            Prim(PrimitiveType.Cylinder, "Pad", go.transform, new Vector3(0f, 0.05f, 0f), new Vector3(3f, 0.05f, 3f), goal, false);
            Prim(PrimitiveType.Cube, "Beacon", go.transform, new Vector3(0f, 1.6f, 0f), new Vector3(0.6f, 3f, 0.6f), goal, false);
            SavePrefab(go, "Goal");
        }

        // Checkpoint
        {
            GameObject go = new GameObject("Checkpoint");
            BoxCollider bc = Trigger<BoxCollider>(go);
            bc.size = new Vector3(3f, 3f, 3f);
            bc.center = new Vector3(0f, 1.5f, 0f);
            Checkpoint cp = go.AddComponent<Checkpoint>();
            Prim(PrimitiveType.Cylinder, "Pole", go.transform, new Vector3(0f, 1f, 0f), new Vector3(0.12f, 1f, 0.12f), pole, false);
            GameObject f = Prim(PrimitiveType.Cube, "Flag", go.transform, new Vector3(0.45f, 1.7f, 0f), new Vector3(0.9f, 0.55f, 0.05f), flag, false);
            SerializedObject so = new SerializedObject(cp);
            so.FindProperty("flag").objectReferenceValue = f.GetComponent<Renderer>();
            so.ApplyModifiedPropertiesWithoutUndo();
            SavePrefab(go, "Checkpoint");
        }

        // Power-ups: a speed boost (timed, runs on the player) and a shield (a bool Health checks).
        {
            GameObject go = new GameObject("PowerUp_Speed");
            SphereCollider sc = Trigger<SphereCollider>(go);
            sc.radius = 0.8f;
            PowerUp pu = go.AddComponent<PowerUp>();
            pu.type = PowerUpType.Speed;
            pu.multiplier = 1.6f;
            pu.duration = 5f;
            SetProp(pu, "pickupEffect", powerFx);
            Prim(PrimitiveType.Sphere, "Orb", go.transform, Vector3.zero, Vector3.one * 0.7f, speedMat, false);
            Prim(PrimitiveType.Cube, "Bolt", go.transform, Vector3.zero, new Vector3(0.15f, 1.1f, 0.15f), speedMat, false).transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
            SavePrefab(go, "PowerUp_Speed");
        }
        {
            GameObject go = new GameObject("PowerUp_Shield");
            SphereCollider sc = Trigger<SphereCollider>(go);
            sc.radius = 0.8f;
            PowerUp pu = go.AddComponent<PowerUp>();
            pu.type = PowerUpType.Shield;
            SetProp(pu, "pickupEffect", powerFx);
            Prim(PrimitiveType.Sphere, "Orb", go.transform, Vector3.zero, Vector3.one * 0.8f, shieldMat, false);
            SavePrefab(go, "PowerUp_Shield");
        }

        // KillZone: one wide trigger under the level.
        {
            GameObject go = new GameObject("KillZone");
            BoxCollider bc = Trigger<BoxCollider>(go);
            bc.size = Vector3.one;
            go.AddComponent<KillZone>();
            SavePrefab(go, "KillZone");
        }

        // GameManager: the prefab keeps only its numbers; the level builder fills in the scene references.
        {
            GameObject go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
            SavePrefab(go, "GameManager");
        }

        BuildHud();
    }
}

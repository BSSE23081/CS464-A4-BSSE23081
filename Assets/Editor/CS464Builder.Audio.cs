using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

// Audio for Assignment 04: the Audio Mixer (Master > Music, SFX; MusicVolume and SfxVolume exposed),
// the clips in Assets/Audio, the particle prefabs, and the SfxPlayer and PersistentMusic prefabs.
public static partial class CS464Builder
{
    const string AudioDir = "Assets/Audio";
    const string MixerPath = "Assets/Audio/MainMixer.mixer";

    static AudioMixer mixer;
    static AudioMixerGroup gMusic, gSfx;

    // ------------------------------------------------------------------ small property helpers
    static void SetProp(Object target, string field, object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null) { Debug.LogError("CS464 builder: no serialized field '" + field + "' on " + target.GetType().Name); return; }
        if (value is Object || value == null) p.objectReferenceValue = (Object)value;
        else if (value is float) p.floatValue = (float)value;
        else if (value is int) { if (p.propertyType == SerializedPropertyType.Enum) p.enumValueIndex = (int)value; else p.intValue = (int)value; }
        else if (value is bool) p.boolValue = (bool)value;
        else if (value is string) p.stringValue = (string)value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetArray(Object target, string field, Object[] values)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static AudioClip Clip(string name)
    {
        AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioDir + "/" + name + ".wav");
        if (c == null) Debug.LogWarning("CS464 builder: Assets/Audio/" + name + ".wav is missing. Run Assets/Tools/gen_audio.py, or copy the Audio folder in.");
        return c;
    }

    static T Prefab<T>(string name) where T : Component
    {
        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/" + name + ".prefab");
        return go != null ? go.GetComponent<T>() : null;
    }

    // ------------------------------------------------------------------ the mixer
    static void LoadMixer()
    {
        mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
        gMusic = null;
        gSfx = null;
        if (mixer == null) return;
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(MixerPath))
        {
            AudioMixerGroup g = o as AudioMixerGroup;
            if (g == null) continue;
            if (g.name == "Music") gMusic = g;
            else if (g.name == "SFX") gSfx = g;
        }
    }

    static void EnsureAudio()
    {
        EnsureFolder("Assets", "Audio");
        AssetDatabase.Refresh();
        LoadMixer();
        if (gMusic == null || gSfx == null)   // no mixer yet (or a broken one): write ours
        {
            System.IO.File.WriteAllText(MixerPath, MixerYaml);
            AssetDatabase.ImportAsset(MixerPath, ImportAssetOptions.ForceUpdate);
            LoadMixer();
        }
        if (mixer == null || gMusic == null || gSfx == null)
        {
            Debug.LogError("CS464 builder: the Audio Mixer could not be created. Make it by hand: Project window > Create > Audio > Audio Mixer, name it MainMixer, " +
                           "save it in Assets/Audio, add two child groups of Master named Music and SFX, right-click each group's Volume > Expose, " +
                           "and rename the exposed parameters to MusicVolume and SfxVolume (Exposed Parameters, top right). Then run this menu command again.");
            EditorUtility.DisplayDialog("CS464", "The Audio Mixer could not be created automatically. The Console says how to make it by hand (2 minutes). Everything else is built.", "OK");
        }
        else
        {
            bool a = mixer.SetFloat("MusicVolume", 0f);
            bool b = mixer.SetFloat("SfxVolume", 0f);
            if (!a || !b) Debug.LogWarning("CS464 builder: the mixer is missing an exposed parameter named MusicVolume or SfxVolume (Exposed Parameters, top right of the Audio Mixer window).");
        }
    }

    // A hand-written AudioMixer asset: Master with two children, Music and SFX, each volume exposed under the name the scripts use.
    const string MixerYaml = @"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!241 &24100000
AudioMixerController:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: MainMixer
  m_OutputGroup: {fileID: 0}
  m_MasterGroup: {fileID: 24300002}
  m_Snapshots:
  - {fileID: 24500010}
  m_StartSnapshot: {fileID: 24500010}
  m_SuspendThreshold: -80
  m_EnableSuspend: 1
  m_UpdateMode: 0
  m_ExposedParameters:
  - guid: f88012e9ebfb4d82b1d707b673461ff7
    name: MusicVolume
  - guid: 5e8de27d13b44ea0a2b1650380907c47
    name: SfxVolume
  m_AudioMixerGroupViews:
  - guids:
    - d32abe4fb6ce416a8092faceef104102
    - acbc8c3d33f846159a62289de618e6e7
    - 2168609e1ac7400ba5510051bf2cf3e9
    name: View
  m_CurrentViewIndex: 0
  m_TargetSnapshot: {fileID: 24500010}
--- !u!243 &24300002
AudioMixerGroupController:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: Master
  m_AudioMixer: {fileID: 24100000}
  m_GroupID: d32abe4fb6ce416a8092faceef104102
  m_Children:
  - {fileID: 24300004}
  - {fileID: 24300006}
  m_Volume: 0e22f33e53074071863ba4dfa0a919f9
  m_Pitch: cdbed1563d794bf5bbb8a2cc0631c231
  m_Send: 00000000000000000000000000000000
  m_Effects:
  - {fileID: 24400002}
  m_UserColorIndex: 0
  m_Mute: 0
  m_Solo: 0
  m_BypassEffects: 0
--- !u!243 &24300004
AudioMixerGroupController:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: Music
  m_AudioMixer: {fileID: 24100000}
  m_GroupID: acbc8c3d33f846159a62289de618e6e7
  m_Children: []
  m_Volume: f88012e9ebfb4d82b1d707b673461ff7
  m_Pitch: 86adf8e946da4acd8b4c382b2019fd94
  m_Send: 00000000000000000000000000000000
  m_Effects:
  - {fileID: 24400004}
  m_UserColorIndex: 0
  m_Mute: 0
  m_Solo: 0
  m_BypassEffects: 0
--- !u!243 &24300006
AudioMixerGroupController:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: SFX
  m_AudioMixer: {fileID: 24100000}
  m_GroupID: 2168609e1ac7400ba5510051bf2cf3e9
  m_Children: []
  m_Volume: 5e8de27d13b44ea0a2b1650380907c47
  m_Pitch: f41f71aa69814f7b9bd0112559f43430
  m_Send: 00000000000000000000000000000000
  m_Effects:
  - {fileID: 24400006}
  m_UserColorIndex: 0
  m_Mute: 0
  m_Solo: 0
  m_BypassEffects: 0
--- !u!244 &24400002
AudioMixerEffectController:
  m_ObjectHideFlags: 3
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: 
  m_EffectID: ea1c713910c140069e33503079962e23
  m_EffectName: Attenuation
  m_MixLevel: 20014f388ba849d688a2a2931a49d82b
  m_Parameters: []
  m_SendTarget: {fileID: 0}
  m_EnableWetMix: 0
  m_Bypass: 0
--- !u!244 &24400004
AudioMixerEffectController:
  m_ObjectHideFlags: 3
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: 
  m_EffectID: f2e95adcb241405da8d6ebfbd37035e5
  m_EffectName: Attenuation
  m_MixLevel: e7a656bf04f548a193504347d066e1a6
  m_Parameters: []
  m_SendTarget: {fileID: 0}
  m_EnableWetMix: 0
  m_Bypass: 0
--- !u!244 &24400006
AudioMixerEffectController:
  m_ObjectHideFlags: 3
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: 
  m_EffectID: 9133dc66b61c459d8cd74433da56f6d1
  m_EffectName: Attenuation
  m_MixLevel: 8fe4e22ab08a4eca89418dad7ee7239b
  m_Parameters: []
  m_SendTarget: {fileID: 0}
  m_EnableWetMix: 0
  m_Bypass: 0
--- !u!245 &24500010
AudioMixerSnapshotController:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: Snapshot
  m_AudioMixer: {fileID: 24100000}
  m_SnapshotID: 4c1e3db1d4394c3992804e66eeb356a1
  m_FloatValues: {}
  m_TransitionOverrides: {}
";

    // ------------------------------------------------------------------ particles
    static Material ParticleMaterial()
    {
        string path = MatDir + "/Particle.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            // Sprites/Default tints by the particle's own colour and renders in every pipeline.
            Shader sh = Shader.Find("Sprites/Default");
            if (sh == null) sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (sh == null) sh = Shader.Find("Standard");
            m = new Material(sh);
            AssetDatabase.CreateAsset(m, path);
        }
        return m;
    }

    // One burst, no loop. destroyWhenDone = Stop Action "Destroy": a pickup effect removes itself.
    static ParticleSystem MakeParticles(string name, Transform parent, Color color, int count, float lifetime, float speed, float size, bool destroyWhenDone, bool playOnAwake)
    {
        GameObject go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);   // settings below need it stopped

        ParticleSystem.MainModule main = ps.main;
        main.duration = lifetime;
        main.loop = false;
        main.playOnAwake = playOnAwake;
        main.startLifetime = lifetime;
        main.startSpeed = speed;
        main.startSize = size;
        main.startColor = color;
        main.gravityModifier = 0.6f;
        main.maxParticles = count * 3;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = destroyWhenDone ? ParticleSystemStopAction.Destroy : ParticleSystemStopAction.None;

        ParticleSystem.EmissionModule em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = g;

        ParticleSystem.SizeOverLifetimeModule sz = ps.sizeOverLifetime;
        sz.enabled = true;
        sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = ParticleMaterial();
        return ps;
    }

    static void BuildEffects()
    {
        SavePrefab(MakeParticles("PickupBurst", null, Palette.Pickup, 16, 0.6f, 3.5f, 0.22f, true, true).gameObject, "PickupBurst");
        SavePrefab(MakeParticles("PowerUpBurst", null, Palette.Button, 24, 0.8f, 4.5f, 0.26f, true, true).gameObject, "PowerUpBurst");
    }

    // ------------------------------------------------------------------ SfxPlayer and PersistentMusic
    static void BuildAudioPrefabs()
    {
        // SfxPlayer: one singleton, four AudioSources on the SFX group.
        {
            GameObject go = new GameObject("SfxPlayer");
            SfxPlayer sp = go.AddComponent<SfxPlayer>();
            AudioSource[] sources = new AudioSource[4];
            for (int i = 0; i < sources.Length; i++)
            {
                sources[i] = go.AddComponent<AudioSource>();
                sources[i].playOnAwake = false;
                sources[i].outputAudioMixerGroup = gSfx;
            }
            SetArray(sp, "sources", sources);
            SetProp(sp, "pitchSemitones", 2f);
            SetProp(sp, "pickup", Clip("sfx_pickup"));
            SetProp(sp, "hit", Clip("sfx_hit"));
            SetProp(sp, "button", Clip("sfx_button"));
            SetProp(sp, "jump", Clip("sfx_jump"));
            SetProp(sp, "bounce", Clip("sfx_bounce"));
            SetProp(sp, "checkpoint", Clip("sfx_checkpoint"));
            SetProp(sp, "powerUp", Clip("sfx_powerup"));
            SetProp(sp, "win", Clip("sfx_win"));
            SetProp(sp, "lose", Clip("sfx_lose"));
            SavePrefab(go, "SfxPlayer");
        }

        // PersistentMusic: one looping AudioSource on the Music group. Play On Awake is OFF (the script starts it).
        {
            GameObject go = new GameObject("PersistentMusic");
            AudioSource src = go.AddComponent<AudioSource>();
            src.clip = Clip("music_theme");
            src.loop = true;
            src.playOnAwake = false;
            src.volume = 0.8f;
            src.outputAudioMixerGroup = gMusic;
            PersistentMusic pm = go.AddComponent<PersistentMusic>();
            SetProp(pm, "source", src);
            SetProp(pm, "mixer", mixer);
            SavePrefab(go, "PersistentMusic");
        }
    }

    // A looping 3D sound on a prefab. rolloff: 0 Logarithmic, 1 Linear.
    static void AddWorldSound(GameObject go, string clip, int rolloff, float min, float max, float volume)
    {
        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = Clip(clip);
        src.loop = true;
        src.playOnAwake = true;
        src.spatialBlend = 1f;
        src.rolloffMode = rolloff == 1 ? AudioRolloffMode.Linear : AudioRolloffMode.Logarithmic;
        src.minDistance = min;
        src.maxDistance = max;
        src.volume = volume;
        src.dopplerLevel = 0f;
        src.outputAudioMixerGroup = gSfx;
        WorldSound ws = go.AddComponent<WorldSound>();
        SetProp(ws, "rolloff", rolloff);
        SetProp(ws, "minDistance", min);
        SetProp(ws, "maxDistance", max);
        SetProp(ws, "volume", volume);
    }
}

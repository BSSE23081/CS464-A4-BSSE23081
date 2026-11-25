# CS464 Assignment 04 · Saleh bint e Bilal · BSSE23081

**Video (90–180 s):** [Watch on Google Drive](https://drive.google.com/file/d/1LWOeOOxNcccPgKACwtCh0JITiLrMLh5V/view?usp=sharing)

<p>
<img src="Docs/hud-16x9.png" width="320" alt="The HUD at 16:9">
<img src="Docs/hud-4x3.png" width="240" alt="The HUD at 4:3">
<img src="Docs/hud-gray.png" width="320" alt="The HUD in grayscale">
</p>

## Settings

| Part | Setting | Value | Why |
|---|---|---|---|
| A | Harmony · base hue | triadic · 258° | Gave me a main colour plus two accents that can't be mistaken for it. Indigo is the UI, orange-red is danger, green is pickups. |
| A | Dark · mid · light | #1B1626 · #A592D1 · #EFEBF7 | Dark is the panels, mid is the buttons, light is the text. The player is almost the same light colour. |
| A | Accents | #BD4C1C · #5CB877 | Orange-red for hazards, green for pickups and the score. They also land on different greys (hazard 0.161, pickup 0.377, player 0.847), so grayscale still works. |
| A | HUD text on its panel | text on panel | 15.02 : 1 (score 7.17 : 1) | Text sits on a solid panel, so the ratio doesn't depend on what's behind it in the level. |
| A | Button label on the button | dark label on #A592D1 | 6.43 : 1 (highlighted 9.96, selected 14.80, pressed 4.66) | Dark label. White on this mid purple wouldn't reach 4.5. |
| B | Music · SFX groups, as mixed | 0 dB · 0 dB | Both at 0 dB so the sliders only turn things down. The music source is at 0.8, which keeps it just under the effects. |
| B | Slider at 1 · 0.5 · 0.0001 | 0 dB · −6.02 dB · −80 dB | The slider is linear but hearing isn't. Raw 0–1 would only move the mixer about 1 dB. The minimum is 0.0001 because Log10(0) breaks. |
| C | Guard hum, in Levels 3–5 | logarithmic · Min 2 m · Max 16 m | Logarithmic is how sound fades in real life. 2 m is about the guard's size. 16 m is past its chase range, so you hear it before it starts chasing you. |
| C | Goal chime, in Levels 2 and 4 | linear · Min 3 m · Max 35 m | Linear, so it fades out completely at 35 m. I wanted it audible from across the level. |
| C | AudioListener | on an empty object that follows the player | The camera sits about 14 m behind the ball, so a listener on it hears everything from too far away. The empty object follows the player and copies the camera's rotation. The camera's own listener is removed. |
| D | unlocked | int · default 1 | Level 1 has to be open on a fresh install. |
| D | best1 … best5 | float · HasKey first | No win means no time. HasKey stops the menu showing 0:00 for a level you haven't beaten. |
| D | MusicVolume | float · default 1 | A fresh install starts at full volume, not silent. SfxVolume works the same way. |

## Answers

1. **What `Time.timeScale = 0` stops in my game, and what it doesn't:** Physics, `Time.deltaTime` and `WaitForSeconds` stop, so the ball, guards and crushers freeze. `Update`, input, the UI and audio keep going. That's why Esc can unpause. It's also why `PlayerController.Update` returns early when `PauseMenu.IsPaused`, otherwise Space on Resume would queue a jump. Music doesn't stop by itself either, so I set `AudioListener.pause` too.

2. **What survives a LoadScene in my game, and how:** The music player and `SfxPlayer` survive on purpose (`DontDestroyOnLoad`), and an `Instance != null` check destroys any duplicate when I go back to the menu. PlayerPrefs survives too. The accidental ones are `Time.timeScale`, `AudioListener.pause` and the static `IsPaused`. `LevelLoader.Load` resets all three first, otherwise a restart from pause comes back frozen or silent.

3. **Why every PlayerPrefs read in my game has a default:** Nothing is saved on a fresh install or after Clear All PlayerPrefs, so a read just hands back the default. `unlocked` defaults to 1, otherwise Level 1 is locked and you can't start. Volumes default to 1 or the game starts silent. Best times check `HasKey` first, so an unplayed level doesn't show 0:00.

4. **An exception a forgotten unsubscribe would throw, and why mine can't:** The `Health` event would still hold a destroyed listener, and the next `Damaged` call after a reload would throw a `MissingReferenceException`. Mine can't. `HealthBar`, `HitFlash`, `DamageSfx`, `HitSparks` and `CameraShake` all do `+=` in `OnEnable` and `-=` in `OnDisable`. The events are also instance events on `Health`, which goes with the scene, not static ones.

## Credits

- All sounds and the music · synthesised by `Assets/Tools/gen_audio.py` · [CS464-A4-BSSE23081](https://github.com/BSSE23081/CS464-A4-BSSE23081) · CC0
- Font · Liberation Sans (TextMesh Pro default) · [github.com/liberationfonts/liberation-fonts](https://github.com/liberationfonts/liberation-fonts) · SIL Open Font License 1.1
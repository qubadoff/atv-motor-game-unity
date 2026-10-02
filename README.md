# Old ATV Motor (BurnGame)

The project folder is named `burngetter`; the game is **Old ATV Motor**, by **BurnGame**.

A black-and-white 2D ATV riding game (Unity 6.3 LTS). 50 levels, racing against the clock: reach the finish flag, and beat the target time for 3 stars.
Touch the ground with your helmet and you crash. Bumps and dips start from level one; let off the throttle in the air or you'll land on your back.

## Stars
- 3 stars: under `length / parSpeed` seconds (`LevelConfig.ThreeStarTime`); 2 stars: within 1.45× of that; 1 star: just finish.
- A live target sits under the HUD timer: stars fade out as the clock passes each threshold.
- Calibration: the crash-free test bot (PlaytestRunner) finishes level 1 in ~58 s (the 3-star line is 64 s). The full-throttle bot (`BURN_BOT=dumb`) flips over at second 15.

## Flow
0. **Splash scene**: the BurnGame logo (a flame inside a tire) and wordmark; cuts to the menu after ~2.5 s (tap to skip).
1. **Menu scene**: asks for a name on first launch, then PLAY (50 levels, 1 unlocked, the rest locked) and SETTINGS (language, music, sound) tabs.
2. **Game scene**: runs the selected level. Reaching the finish flag unlocks the next level and records your best time.
   Motivational quotes appear in the sky while you ride (4 per run), with clouds and flocks of birds drifting past.

## Languages
Azerbaijani, English, Russian, Turkish. The default is **Auto**: the device language (`Loc.DetectDeviceLanguage` — OS locale first, then Unity's systemLanguage).
A fixed language can be chosen in Settings; picking "Auto" returns to the device language.
Strings live in `Assets/Scripts/Loc.cs`, the motivational quotes in `Assets/Scripts/Quotes.cs`.

## Music
The tracks under `Assets/Audio/Music` are by Kevin MacLeod (CC BY 4.0), see `CREDITS.md`. The menu scene's `MusicPlayer` survives scene changes and shuffles the playlist.

## Controls
- **Throttle:** Right arrow / D / W; the right half of the screen on touch
- **Brake / reverse:** Left arrow / A / S; the left half on touch
- In the air, throttle lifts the nose and brake drops it.
- Game over: **Retry** (R / Space) or **Menu** (Esc). Level complete: **Next level** (Space).

## Project layout
- `Assets/Scenes/Menu.unity`, `Assets/Scenes/Game.unity`
- `Assets/Scripts/AtvController.cs` — engine, brakes, lean, anti-flip, input
- `Assets/Scripts/EngineAudio.cs` — engine sound synthesized in code (no audio files)
- `Assets/Scripts/Levels.cs` — the 50-level difficulty curve and `LevelSession`
- `Assets/Scripts/PlayerProfile.cs` — name, unlocked levels, best times (PlayerPrefs)
- `Assets/Scripts/TerrainGenerator.cs` — per-level terrain (Perlin + bumps), flattens after the finish
- `Assets/Scripts/GameManager.cs` — HUD, finish flag, level completion, game over
- `Assets/Scripts/MenuController.cs` — name screen, PLAY/SETTINGS tabs (UI built in code)
- `Assets/Scripts/Loc.cs`, `Quotes.cs`, `Settings.cs`, `MusicPlayer.cs` — language, quotes, settings, music
- `Assets/Scripts/QuoteDisplay.cs`, `SkyDecor.cs` — sky quotes, clouds and birds
- `Assets/Editor/SpriteFactory.cs` — draws the ATV, wheel, flag, lock, cloud and bird sprites in code (preview: `Previews/atv_preview.png`)
- `Assets/Editor/TerrainAudit.cs` — measures each level's maximum slope (menu: **BurnGetter > Terrain Audit**)
- `Assets/Editor/SceneBuilder.cs` — builds both scenes from scratch (menu: **BurnGetter > Build Game Scenes**)
- `Assets/Editor/PlaytestRunner.cs` — automated ride test from the command line

## Command line
```
UNITY="/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity"
# Rebuild sprites and scenes from scratch (the editor must be closed)
"$UNITY" -batchmode -projectPath "$PWD" -executeMethod SceneBuilder.Build -quit -logFile build.log
# Automated test: ride to the finish (smart bot) on the chosen level; BURN_BOT=dumb for the throttle-only bot
BURN_LEVEL=25 "$UNITY" -batchmode -projectPath "$PWD" -executeMethod PlaytestRunner.Run -logFile playtest.log
# Menu scene: text overflow/overlap audit across every language x (name, play, settings) page (UiAudit)
BURN_SCENE=Menu "$UNITY" -batchmode -projectPath "$PWD" -executeMethod PlaytestRunner.Run -logFile menu.log
```
Note: changing a script's defaults does not update values already saved in a scene; rebuild the scenes with `SceneBuilder.Build`.

## Android APK
```
"$UNITY" -batchmode -buildTarget Android -projectPath "$PWD" -executeMethod BuildScript.BuildAndroid -quit -logFile android.log
```
Output: `Builds/Android/OldAtvMotor.apk` (IL2CPP, ARM64 + ARMv7, minSdk 24). Menu: **BurnGetter > Build Android APK**.

## App identity
- iOS: `org.burngame.atv-motor`, Android: `org.burngame.atvmotor` (a hyphen is invalid in an Android package name); company: BurnGame.
- The app icon is `Assets/Sprites/logo.png`; the Unity splash logo is off (free on every license since Unity 6).

## Tuning tips
- Physics: `AtvController` on the `ATV` object — `motorTorque`, `maxSpeed`, `airTorque`, `antiFlipAngle`
- Difficulty curve: `Levels.Get` in `Assets/Scripts/Levels.cs`
- Audio: `EngineAudio` on the `ATV` object — `idlePitch`, `maxPitch`, `throttleVolume`

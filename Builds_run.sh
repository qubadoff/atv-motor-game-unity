#!/bin/zsh
cd /Users/virus/Documents/GameProjects/burngetter
UNITY="/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity"
S=/private/tmp/claude-501/-Users-virus-Documents-GameProjects-burngetter/d68ccc01-2f1e-4840-a60d-6e5825e9083e/scratchpad
"$UNITY" -batchmode -projectPath "$PWD" -executeMethod SceneBuilder.Build -quit -logFile "$S/build.log"; echo "sahne exit=$?"; grep -E "error CS" "$S/build.log" | head -3
BURN_LEVEL=1 "$UNITY" -batchmode -projectPath "$PWD" -executeMethod PlaytestRunner.Run -logFile "$S/playtest_1.log"; echo "oyun exit=$?"; grep -E "UIAUDIT.*(TASMA|BINME)|PLAYTEST: (bitis|OK|FAIL)" "$S/playtest_1.log" | cut -c1-160
BURN_SCENE=Menu "$UNITY" -batchmode -projectPath "$PWD" -executeMethod PlaytestRunner.Run -logFile "$S/playtest_menu.log"; echo "menu exit=$?"; grep -E "UIAUDIT.*(TASMA|BINME)|PLAYTEST: (OK|FAIL)" "$S/playtest_menu.log" | cut -c1-160
echo "=== APK: $(date)"
"$UNITY" -batchmode -nographics -buildTarget Android -projectPath "$PWD" -executeMethod BuildScript.BuildAndroid -quit -logFile "$S/android.log"; echo "apk exit=$?"
grep -E "BUILD:|error CS|Error building" "$S/android.log" | head -5
cp Builds/Android/OldAtvMotor.apk ~/Downloads/OldAtvMotor.apk && ls -la ~/Downloads/OldAtvMotor.apk
ADBBIN="/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb"
a() { "$ADBBIN" -s emulator-5554 "$@"; }
a install -r ~/Downloads/OldAtvMotor.apk 2>&1 | tail -1
a shell am start -n org.burngame.atvmotor/com.unity3d.player.UnityPlayerGameActivity >/dev/null 2>&1; sleep 8
a exec-out screencap -p > "$S/emu_levels.png"; sips -Z 1200 "$S/emu_levels.png" --out "$S/emu_levels_s.png" >/dev/null
a shell input tap 506 389; sleep 3
a shell input swipe 2000 800 2000 800 9000 & sleep 7; a exec-out screencap -p > "$S/emu_game1.png"; wait
sips -Z 1200 "$S/emu_game1.png" --out "$S/emu_game1_s.png" >/dev/null
rm -f Temp/UnityLockfile; open -a "/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app" --args -projectPath "$PWD" && echo "editor launched"

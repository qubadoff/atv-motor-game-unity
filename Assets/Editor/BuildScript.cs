using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Komut satirindan derleme: -buildTarget Android -executeMethod BuildScript.BuildAndroid</summary>
public static class BuildScript
{
    [MenuItem("BurnGetter/Build Android APK")]
    public static void BuildAndroid()
    {
        string dir = "Builds/Android";
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, "OldAtvMotor.apk");
        EditorUserBuildSettings.buildAppBundle = false;
        var scenes = new[] { "Assets/Scenes/Splash.unity", "Assets/Scenes/Menu.unity", "Assets/Scenes/Game.unity" };
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = path,
            target = BuildTarget.Android,
            options = BuildOptions.None,
        };
        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;
        Debug.Log("BUILD: sonuc=" + summary.result + " boyut=" + (summary.totalSize / (1024 * 1024)) + "MB sure=" + summary.totalTime.TotalSeconds.ToString("F0") + "s hata=" + summary.totalErrors + " yol=" + path);
        if (summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) EditorApplication.Exit(1);
    }
}

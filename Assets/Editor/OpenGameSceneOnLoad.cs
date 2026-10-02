using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>Editor acilinca bos sahne yerine oyun sahnesini acar.</summary>
[InitializeOnLoad]
static class OpenGameSceneOnLoad
{
    static OpenGameSceneOnLoad()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
                EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
        };
    }
}

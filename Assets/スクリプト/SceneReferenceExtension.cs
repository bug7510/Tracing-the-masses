using UnityEngine.SceneManagement;
using Eflatun.SceneReference;
using System.Threading;

static class SceneReferenceExtension
{
    public static void LoadScene(this SceneReference targetSceneRef, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(targetSceneRef.Name, loadSceneMode);
    }
}
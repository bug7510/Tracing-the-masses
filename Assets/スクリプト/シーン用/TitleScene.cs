using Eflatun.SceneReference;
using UnityEngine;

public class TitleScene : MonoBehaviour
{
    [SerializeField] SceneReference GameScene;
    public void StartGame()
    {
        GameScene.LoadScene();
    }
    public void Quit()
    {
        Application.Quit();
    }
}

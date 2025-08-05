using DG.Tweening;
using Eflatun.SceneReference;
using UnityEngine;

public class OverScene : MonoBehaviour
{
    [SerializeField] CanvasGroup buttonGroup;
    void Start()
    {
        buttonGroup.alpha = 0;
        DOTween.Sequence().AppendInterval(0.3f)
                            .Append(buttonGroup.DOFade(1.0f, 1.0f));
    }
    [SerializeField] SceneReference GameScene;
    public void Retry()
    {
        GameScene.LoadScene();
    }
    // public void Quit()
    // {
    //     Application.Quit();
    // }
    [SerializeField] SceneReference titleScene;
    public void Quit()
    {
        titleScene.LoadScene();
    }
}

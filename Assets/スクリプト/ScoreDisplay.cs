using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreDisplay;
    void Awake()
    {
        scoreDisplay = GetComponent<TextMeshProUGUI>();
        completeRect = completeDisplay.rectTransform;
    }
    Tween rotate;
    public void Rotate(int angle)
    {
        if (rotate != null && rotate.IsPlaying()) rotate.Complete();
        rotate = transform.DORotate(new(0, 0, angle + transform.rotation.eulerAngles.z), 2f)
                            .SetEase(Ease.InOutBack)
                            .SetDependency(() => GameTime.isGaming);
    }
    public void Score(int score)
    {
        scoreDisplay.text = score.ToString();
    }
    [SerializeField] TextMeshProUGUI completeDisplay;
    RectTransform completeRect;
    public void Complete()
    {
        scoreDisplay.gameObject.SetActive(true);
        DOTween.Sequence().Append(completeRect.DOAnchorPos(new(0, 25), 0.5f))
                            .Join(completeDisplay.DOFade(1.0f, 0.5f))
                            .AppendInterval(0.5f)
                            .Append(completeRect.DOAnchorPos(Vector2.zero, 0.5f))
                            .Join(completeDisplay.DOFade(0f, 0.5f))
                            .Play().SetDependency(() => GameTime.isGaming);
    }
    [SerializeField] TextMeshProUGUI isScoreDisplay;
    [SerializeField] float duration;
    public void PhaseText()
    {
        if (GameSceneManager.phase == Phase.tracing)
        {
            isScoreDisplay.DOColor(Color.white, duration).SetDependency(() => GameTime.isGaming);
            scoreDisplay.DOColor(Color.white, duration).SetDependency(() => GameTime.isGaming);
        }
        else
        {
            isScoreDisplay.DOColor(Color.black, duration).SetDependency(() => GameTime.isGaming);
            scoreDisplay.DOColor(Color.black, duration).SetDependency(() => GameTime.isGaming);
        }
    }
}

using DG.Tweening;
using TMPro;
using UnityEngine;
public class CompleteCounter : MonoBehaviour
{
    TextMeshProUGUI completeDisplay;
    void Awake()
    {
        completeDisplay = GetComponent<TextMeshProUGUI>();
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
        completeDisplay.text = score.ToString();
    }

    [SerializeField] float duration;
    public void PhaseText()
    {
        if (GameSceneManager.phase == Phase.tracing)
        {
            completeDisplay.DOColor(Color.white, duration).SetDependency(() => GameTime.isGaming);
        }
        else
        {
            completeDisplay.DOColor(Color.black, duration).SetDependency(() => GameTime.isGaming);
        }
    }
}
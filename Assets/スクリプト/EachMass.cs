using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EachMass : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    Vector2Int position = new();
    public void Set(int x, int y, Action<Vector2Int> eventHandler)
    {
        position = new(x, y);
        On_Click += eventHandler;
    }
    public void Reset()
    {
        position = Vector2Int.zero;
        On_Click = null;
    }
    event Action<Vector2Int> On_Click;
    Image thisImage;
    void Awake()
    {
        thisImage = GetComponent<Image>();
    }
    public readonly static Color tracingColor = Color.white;
    public readonly static Color modelingColor = Color.black;
    public void EachPhase()
    {
        if (GameSceneManager.phase == Phase.tracing)
        {
            ColorChange(tracingColor);
        }
        else
        {
            ColorChange(modelingColor);
        }
    }
    public void EachFade()
    {
        ColorChangeSeq?.Kill();
        if (GameSceneManager.phase == Phase.tracing)
        {
            ColorChangeSeq = DOTween.Sequence().AppendInterval(1f)
                                                .Append(thisImage.DOColor(tracingColor, duration).Play())
                                                .SetDependency(() => GameTime.isGaming);

        }
        else
        {
            ColorChangeSeq = DOTween.Sequence().AppendInterval(1f)
                                                .Append(thisImage.DOColor(modelingColor, duration).Play())
                                                .SetDependency(() => GameTime.isGaming);

        }
    }
    public float duration;
    Sequence ColorChangeSeq;
    public void ColorChange(Color color)
    {
        ColorChangeSeq?.Pause();
        ColorChangeSeq?.Kill();
        ColorChangeSeq = DOTween.Sequence().Append(thisImage.DOColor(color, duration).Play())
                                            .SetDependency(() => GameTime.isGaming);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) On_Click.Invoke(position);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AllMass.CanClick) ColorChange(new Color32(255, 255, 0, 100));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (AllMass.CanClick) EachPhase();
    }
}

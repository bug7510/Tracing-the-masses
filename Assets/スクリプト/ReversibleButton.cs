using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ReversibleButton : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI startButtonTMP;
    [SerializeField] protected Image bigSphere;
    [SerializeField] protected Image smallSphere;
    [SerializeField] Color mainColor;
    protected virtual Color MainColor
    {
        set { mainColor = value; }
        get => mainColor;
    }
    [SerializeField] Color subColor;
    protected virtual Color SubColor
    {
        get => subColor;
    }
    [SerializeField] UnityEvent onClick;
    public virtual void OnClick() { onClick.Invoke(); }
    public void OnCursorInOrOut(bool isCursorIn)
    {
        if (isCursorIn)
        {
            startButtonTMP.color = SubColor;
            bigSphere.color = SubColor;
            smallSphere.color = MainColor;
        }
        else
        {
            startButtonTMP.color = MainColor;
            bigSphere.color = MainColor;
            smallSphere.color = SubColor;
        }
    }

}

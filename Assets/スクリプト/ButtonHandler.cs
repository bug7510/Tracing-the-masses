using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ButtonHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] UnityEvent<bool> onCursorInOrOut;
    [SerializeField] UnityEvent onClick;
    [SerializeField] InputActionReference onKeyReference;
    InputAction onKey;
    bool isEnabledButton = true;
    public bool IsEnabledButton
    {
        set
        {
            if (!value) onCursorInOrOut?.Invoke(false);
            isEnabledButton = value;
        }
        get => isEnabledButton;
    }
    void Start()
    {
        if (onKeyReference != null)
        {
            onKey = onKeyReference.action;
            onKey.Enable();
            onKey.performed += OnKey;
        }
    }
    void OnEnable()
    {
        onCursorInOrOut.Invoke(false);
    }
    void OnKey(InputAction.CallbackContext context)
    {
        if (IsEnabledButton && onKeyReference != null)
        {
            onClick.Invoke();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsEnabledButton)
        {
            onCursorInOrOut.Invoke(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsEnabledButton)
        {
            onCursorInOrOut.Invoke(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right && IsEnabledButton)
        {
            onClick.Invoke();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LeftRightClickable : PointerManipulator
{
    public Action<PointerDownEvent> OnLeftClick { get; set; }
    public Action<PointerDownEvent> OnRightClick { get; set; }

    public LeftRightClickable(Action<PointerDownEvent> onLeftClick, Action<PointerDownEvent> onRightClick)
    {
        OnLeftClick = onLeftClick;
        OnRightClick = onRightClick;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
    }
    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
    }

    private void PointerDownHandler(PointerDownEvent evt)
    {
        if (evt.button == (int)MouseButton.LeftMouse)
        {
            OnLeftClick?.Invoke(evt);
            return;
        }
        if (evt.button == (int)MouseButton.RightMouse)
        {
            OnRightClick?.Invoke(evt);
            return;
        }
    }
}


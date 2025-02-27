using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Handles mouse inputs on the bg of the canvas. This disables selection when clicked, and handles drag and zoom.
/// </summary>
public class ForceDirectedCanvasBGManipulator : PointerManipulator
{
    private bool _enabled;
    private Vector2 _targetStartPosition { get; set; }
    private Vector3 _pointerStartPosition { get; set; }

    public Action OnLeftClick { get; set; }
    public Action OnRightClick { get; set; }
    public Action<Vector2> OnDrag { get; set; }
    private VisualElement _translationContainer;

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
        target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
    }
    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
        target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
    }

    private void PointerDownHandler(PointerDownEvent evt)
    {
        _targetStartPosition = target.transform.position;
        _pointerStartPosition = evt.position;
        if (evt.button == (int)MouseButton.LeftMouse)
        {
            OnLeftClick?.Invoke();
        }
        if (evt.button == (int)MouseButton.RightMouse)
        {
            OnRightClick?.Invoke();
            return;
        }
        if (evt.button == (int)MouseButton.MiddleMouse || evt.button == (int)MouseButton.LeftMouse)
        {
            _enabled = true;
            PointerCaptureHelper.CapturePointer(target, evt.pointerId);
            return;
        }
    }

    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (_enabled && target.HasPointerCapture(evt.pointerId))
        {
            OnDrag?.Invoke(evt.deltaPosition);
        }
    }

    private void PointerUpHandler(PointerUpEvent evt)
    {
        if (_enabled && target.HasPointerCapture(evt.pointerId))
        {
            _enabled = false;
            target.ReleasePointer(evt.pointerId);
        }
    }
}

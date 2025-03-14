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
    private bool _isLeftDrag;
    private Vector2 _targetStartPosition { get; set; }
    private Vector3 _pointerStartPosition { get; set; }

    public Action OnLeftClick { get; set; }
    public Action<Vector2> OnRightClick { get; set; }

    public Action<Vector2> OnLeftDragStart { get; set; }
    public Action<Vector2> OnLeftDrag { get; set; }
    public Action<Vector2> OnLeftDragEnd { get; set; }

    public Action<Vector2> OnMiddleDrag { get; set; }
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
            OnRightClick?.Invoke(evt.position);
            return;
        }
        if (evt.button == (int)MouseButton.MiddleMouse || evt.button == (int)MouseButton.LeftMouse)
        {
            _enabled = true;
            PointerCaptureHelper.CapturePointer(target, evt.pointerId);
            _isLeftDrag = evt.button == (int)MouseButton.LeftMouse;
            if (_isLeftDrag)
            {
                OnLeftDragStart?.Invoke(evt.position);
            }
            return;
        }
    }

    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (_enabled && target.HasPointerCapture(evt.pointerId))
        {
            if (_isLeftDrag)
            {
                OnLeftDrag?.Invoke(evt.deltaPosition);
            }
            else
            {
                OnMiddleDrag?.Invoke(evt.deltaPosition);
            }
        }
    }

    private void PointerUpHandler(PointerUpEvent evt)
    {
        if (_enabled && target.HasPointerCapture(evt.pointerId))
        {
            _enabled = false;
            target.ReleasePointer(evt.pointerId);
            if (_isLeftDrag)
            {
                OnLeftDragEnd?.Invoke(evt.position);
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Container for modifier key states during events.
/// </summary>
public struct DragEventModifiers
{
    public bool shift;
    public bool ctrl;
    public bool alt;

    public DragEventModifiers(bool shift, bool ctrl, bool alt)
    {
        this.shift = shift;
        this.ctrl = ctrl;
        this.alt = alt;
    }
}

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

    public Action<Vector2, DragEventModifiers> OnLeftDragStart { get; set; }
    public Action<Vector2, Vector2> OnLeftDrag { get; set; }  // (delta, currentPos)
    public Action<Vector2, DragEventModifiers> OnLeftDragEnd { get; set; }

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
                var modifiers = new DragEventModifiers(evt.shiftKey, evt.ctrlKey, evt.altKey);
                OnLeftDragStart?.Invoke(evt.position, modifiers);
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
                OnLeftDrag?.Invoke(evt.deltaPosition, evt.position);
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
                var modifiers = new DragEventModifiers(evt.shiftKey, evt.ctrlKey, evt.altKey);
                OnLeftDragEnd?.Invoke(evt.position, modifiers);
            }
        }
    }
}

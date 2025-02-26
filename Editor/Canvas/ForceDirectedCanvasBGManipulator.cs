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

    public ForceDirectedCanvasBGManipulator(VisualElement translationContainer)
    {
        _translationContainer = translationContainer;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
        target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
        target.RegisterCallback<WheelEvent>(WheelHandler);
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

    private void WheelHandler(WheelEvent evt)
    {
        float delta = evt.delta.y;

        if (delta != 0)
        {
            Vector3 mp = evt.mousePosition;
            Vector3 b = _translationContainer.WorldToLocal(mp);

            float zoom = EditorPrefs.GetFloat(ForceDirectedCanvasSettings.ZOOM_KEY, ForceDirectedCanvasSettings.DEFAULT_ZOOM);
            zoom += delta * -.08f * zoom;
            zoom = Mathf.Clamp(zoom, ForceDirectedCanvasSettings.ZOOM_RANGE.x, ForceDirectedCanvasSettings.ZOOM_RANGE.y);
            EditorPrefs.SetFloat(ForceDirectedCanvasSettings.ZOOM_KEY, zoom);
            Vector3 desiredScale = Vector3.one * EditorPrefs.GetFloat(ForceDirectedCanvasSettings.ZOOM_KEY, ForceDirectedCanvasSettings.DEFAULT_ZOOM);
            _translationContainer.transform.scale = desiredScale;
            Vector3 a = _translationContainer.WorldToLocal(mp);

            Vector3 d = (Vector3)_translationContainer.LocalToWorld(a - b) - _translationContainer.worldTransform.GetPosition();
            _translationContainer.transform.position = _translationContainer.transform.position + (d);
            // we are recording mouse positin.
            // zooming the canvas.
            // then seeing the new mouse position.
            // and applying the diff. But in weird local-world space conversions

            // the goal here is to lock the canvas on them mouse position. and scale around it. It works :)
        }
    }
}

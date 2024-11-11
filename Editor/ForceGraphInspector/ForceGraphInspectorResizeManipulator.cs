using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Less3.ForceGraph.Editor
{
    /// <summary>
    /// Manipulator for resizing the size of the graph area in the force graph inspector.
    /// </summary>
    public class ForceGraphInspectorResizeManipulator : PointerManipulator
    {
        private bool _enabled;
        private Vector2 _targetStartPosition { get; set; }
        private Vector3 _pointerStartPosition { get; set; }
        private float _startHeight;

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
            _startHeight = EditorPrefs.GetFloat(ForceGraphInspector.HEIGHT_SETTING_KEY, ForceGraphInspector.DEFAULT_GRAPH_HEIGHT);
            if (evt.button == (int)MouseButton.LeftMouse)
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
                float diff = evt.position.y - _pointerStartPosition.y;
                EditorPrefs.SetFloat(ForceGraphInspector.HEIGHT_SETTING_KEY, Mathf.Max(ForceGraphInspector.MIN_GRAPH_HEIGHT, _startHeight + diff));
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
}

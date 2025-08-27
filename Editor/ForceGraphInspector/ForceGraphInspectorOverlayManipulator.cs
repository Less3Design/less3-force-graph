using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Less3.ForceGraph.Editor
{
    public class ForceGraphInspectorOverlayManipulator : PointerManipulator
    {
        private bool _enabled;// track if an event started inside the root
        private Vector2 _targetStartPosition { get; set; }
        private Vector3 _pointerStartPosition { get; set; }
        private VisualElement _root;

        public ForceGraphInspectorOverlayManipulator(VisualElement root)
        {
            _root = root;
        }

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
            _targetStartPosition = _root.transform.position;
            _pointerStartPosition = evt.position;
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
                Vector3 pointerDelta = evt.position - _pointerStartPosition;
                Vector2 newPos = new Vector2(_targetStartPosition.x + pointerDelta.x, _targetStartPosition.y + pointerDelta.y);

                // clamp to panel
                newPos.x = Mathf.Clamp(newPos.x, 0, target.panel.visualTree.worldBound.width - _root.layout.width - 24);
                newPos.y = Mathf.Clamp(newPos.y, 0, target.panel.visualTree.worldBound.height - _root.layout.height - 48);

                EditorPrefs.SetFloat(ForceGraphInspector.OVERLAY_X_SETTINGS_KEY, newPos.x);
                EditorPrefs.SetFloat(ForceGraphInspector.OVERLAY_Y_SETTINGS_KEY, newPos.y);

                _root.transform.position = newPos;
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

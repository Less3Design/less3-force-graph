using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Less3.ForceGraph.Editor
{
    public class GroupManipulator : PointerManipulator
    {
        private bool _enabled;// track if an event started inside the root
        private Vector2 _targetStartPosition { get; set; }
        private Vector3 _pointerStartPosition { get; set; }

        private ForceCanvasGroupBase _group;
        private IForceDirectedCanvasGeneric _canvas;

        private Action<ForceCanvasGroupBase> _leftClickAction;
        private Action<ForceCanvasGroupBase> _rightClickAction;

        public GroupManipulator(
            ForceCanvasGroupBase node,
            IForceDirectedCanvasGeneric c,
            Action<ForceCanvasGroupBase> leftClickAction,
            Action<ForceCanvasGroupBase> rightClickAction)
        {
            _group = node;
            _canvas = c;
            _leftClickAction = leftClickAction;
            _rightClickAction = rightClickAction;
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
            _targetStartPosition = _group.position;
            _pointerStartPosition = evt.position;
            if (evt.button == (int)MouseButton.RightMouse)
            {
                //_rightClickAction?.Invoke(_node);
                return;
            }
            if (evt.button == (int)MouseButton.LeftMouse)
            {
                _enabled = true;
                PointerCaptureHelper.CapturePointer(target, evt.pointerId);
                //_node.element.Q("Border").AddToClassList("Pressed");

                //_leftClickAction?.Invoke(_node);
                return;
            }
        }

        private void PointerMoveHandler(PointerMoveEvent evt)
        {
            if (_enabled && target.HasPointerCapture(evt.pointerId))
            {
                Vector3 pointerDelta = evt.position - _pointerStartPosition;
                pointerDelta = pointerDelta * (1f / EditorPrefs.GetFloat(ForceDirectedCanvasSettings.ZOOM_KEY, ForceDirectedCanvasSettings.DEFAULT_ZOOM));
                Vector2 newPos = new Vector2(_targetStartPosition.x + pointerDelta.x, _targetStartPosition.y + pointerDelta.y);

                if (EditorPrefs.GetBool(ForceDirectedCanvasSettings.SNAP_SETTINGS_KEY, true))
                {
                    //newPos = _canvas.TryGetNodeSnapPosition(newPos, _node);
                }
                _group.SetPosition(newPos);
            }
        }

        private void PointerUpHandler(PointerUpEvent evt)
        {
            if (_enabled && target.HasPointerCapture(evt.pointerId))
            {
                _enabled = false;
                target.ReleasePointer(evt.pointerId);
                //_node.element.Q("Border").RemoveFromClassList("Pressed");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Less3.ForceGraph.Editor
{
    public class LGroupManipulator<N, C, G> : PointerManipulator where N : class where C : class where G : class
    {
        private bool _enabled;// track if an event started inside the root
        private Vector2 _targetStartPosition { get; set; }
        private Vector3 _pointerStartPosition { get; set; }

        private LCanvasGroup<G, N> _group;
        private LCanvas<N, C, G> _canvas;

        private Dictionary<LCanvasNode<N>, Vector2> _nodesStartPositions = new Dictionary<LCanvasNode<N>, Vector2>();

        private Action<LCanvasGroup<G, N>> _leftClickAction;
        private Action<LCanvasGroup<G, N>> _rightClickAction;

        public LGroupManipulator(
            LCanvasGroup<G, N> node,
            LCanvas<N, C, G> c,
            Action<LCanvasGroup<G, N>> leftClickAction,
            Action<LCanvasGroup<G, N>> rightClickAction)
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

            _nodesStartPositions.Clear();
            foreach (var node in _group.nodes)
            {
                _nodesStartPositions[node] = node.position;
            }

            if (evt.button == (int)MouseButton.RightMouse)
            {
                _rightClickAction?.Invoke(_group);
                return;
            }
            if (evt.button == (int)MouseButton.LeftMouse)
            {
                _enabled = true;
                PointerCaptureHelper.CapturePointer(target, evt.pointerId);
                //_node.element.Q("Border").AddToClassList("Pressed");

                _leftClickAction?.Invoke(_group);
                return;
            }
        }

        private void PointerMoveHandler(PointerMoveEvent evt)
        {
            if (_enabled && target.HasPointerCapture(evt.pointerId))
            {
                Vector3 pointerDelta = evt.position - _pointerStartPosition;
                pointerDelta = pointerDelta * (1f / EditorPrefs.GetFloat(LCanvasPrefs.ZOOM_KEY, LCanvasPrefs.DEFAULT_ZOOM));
                Vector2 newPos = new Vector2(_targetStartPosition.x + pointerDelta.x, _targetStartPosition.y + pointerDelta.y);
                _group.SetPosition(newPos);

                for (int i = 0; i < _group.nodes.Count; i++)
                {
                    LCanvasNode<N> node = _group.nodes[i];
                    Vector2 startPos = _nodesStartPositions[node];
                    Vector2 nodePosition = new Vector2(startPos.x + pointerDelta.x, startPos.y + pointerDelta.y);
                    node.SetPosition(nodePosition);
                }
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

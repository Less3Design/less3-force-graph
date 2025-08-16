using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Less3.ForceGraph.Editor
{
    // Handle making auto connections from the handle on the node.
    // ! Super messy, but it works.
    // TODO cleanup how this and the existing connections work together.
    public class ForceNodeAutoConnectionDragManipulator<N, C, G> : PointerManipulator where N : class where C : class where G : class
    {
        private bool _enabled;// track if an event started inside the root
        private LCanvasGroup<G, N> _hoveredGroup;

        private Vector2 _targetStartPosition { get; set; }
        private Vector3 _pointerStartPosition { get; set; }
        private LCanvasNode<N> _node;

        private LCanvas<N, C, G> _canvas;

        public ForceNodeAutoConnectionDragManipulator(
            LCanvasNode<N> node,
            LCanvas<N, C, G> c)
        {
            _node = node;
            _canvas = c;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
            target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
            target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
        }

        private void PointerDownHandler(PointerDownEvent evt)
        {
            _targetStartPosition = target.transform.position;
            _pointerStartPosition = evt.position;
            if (evt.button == (int)MouseButton.LeftMouse)
            {
                if (_canvas.newConnectionFromNode != null)
                {
                    // a new connection is in progress the normal way.
                    return;
                }

                _enabled = true;
                _canvas.newConnectionFromNode = _node;
                PointerCaptureHelper.CapturePointer(target, evt.pointerId);
                //_node.element.Q("Border").AddToClassList("Pressed");
                _node.element.Q("Border").AddToClassList("CreatingConnection");
                evt.StopPropagation();
                return;
            }
        }

        private void PointerUpHandler(PointerUpEvent evt)
        {
            if (_enabled && target.HasPointerCapture(evt.pointerId))
            {
                _enabled = false;
                target.ReleasePointer(evt.pointerId);

                List<VisualElement> hoveredElements = new List<VisualElement>();
                _canvas.panel.PickAll(new Vector2(evt.position.x, evt.position.y), hoveredElements);

                foreach (var element in hoveredElements)
                {
                    if (_canvas.ElementIsNode(element, out var node))
                    {
                        _canvas.TryCreateAutoConnection(_node, node);
                        break;
                    }
                }

                _canvas.newConnectionFromNode = null;
                _node.element.Q("Border").RemoveFromClassList("CreatingConnection");
                evt.StopPropagation();
            }
        }
    }
}

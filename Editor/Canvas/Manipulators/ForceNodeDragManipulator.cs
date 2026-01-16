using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Less3.ForceGraph.Editor
{
    // N = Node type, C = Connection type, G = Group type
    public class ForceNodeDragManipulator<N, C, G> : PointerManipulator where N : class where C : class where G : class
    {
        private bool _enabled;// track if an event started inside the root
        private LCanvasGroup<G, N> _hoveredGroup;

        private Vector2 _targetStartPosition { get; set; }
        private Vector2 _dataStartPosition { get; set; }
        private Vector3 _pointerStartPosition { get; set; }
        private LCanvasNode<N> _node;

        // Multi-node drag state
        private Dictionary<LCanvasNode<N>, Vector2> _selectedNodesStartPositions = new();
        private bool _isDraggingMultiple;

        private Action<LCanvasNode<N>, bool, bool> _leftClickAction;
        private Action<LCanvasNode<N>> _rightClickAction;

        private Action<LCanvasNode<N>> _enterAction;
        private Action<LCanvasNode<N>> _exitAction;
        private Action<LCanvasNode<N>, Vector2> _dragEndAction;

        private LCanvas<N, C, G> _canvas;

        public ForceNodeDragManipulator(
            LCanvasNode<N> node,
            LCanvas<N, C, G> c,
            Action<LCanvasNode<N>, bool, bool> leftClickAction,
            Action<LCanvasNode<N>> rightClickAction,
            Action<LCanvasNode<N>> enterAction,
            Action<LCanvasNode<N>> exitAction,
            Action<LCanvasNode<N>, Vector2> dragEndAction = null)
        {
            _node = node;
            _canvas = c;
            _leftClickAction = leftClickAction;
            _rightClickAction = rightClickAction;
            _enterAction = enterAction;
            _exitAction = exitAction;
            _dragEndAction = dragEndAction;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
            target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
            target.RegisterCallback<PointerEnterEvent>(PointerEnterHandler);
            target.RegisterCallback<PointerOutEvent>(PointerOutHandler);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
            target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
            target.UnregisterCallback<PointerEnterEvent>(PointerEnterHandler);
            target.UnregisterCallback<PointerOutEvent>(PointerOutHandler);
        }

        private void PointerDownHandler(PointerDownEvent evt)
        {
            _targetStartPosition = _node.element.transform.position;
            _dataStartPosition = _node.position;
            _pointerStartPosition = evt.position;
            if (evt.button == (int)MouseButton.RightMouse)
            {
                _rightClickAction?.Invoke(_node);
                return;
            }
            if (evt.button == (int)MouseButton.LeftMouse)
            {
                _enabled = true;
                PointerCaptureHelper.CapturePointer(target, evt.pointerId);
                _node.element.Q("Border").AddToClassList("Pressed");

                // Store start positions for multi-node drag
                _selectedNodesStartPositions.Clear();
                _isDraggingMultiple = _canvas.IsNodeSelected(_node) && _canvas.selectedNodes.Count > 1;

                if (_isDraggingMultiple)
                {
                    // Store positions of all selected nodes
                    foreach (var selectedNode in _canvas.selectedNodes)
                    {
                        _selectedNodesStartPositions[selectedNode] = selectedNode.position;
                    }
                }
                else
                {
                    // Store only this node's position
                    _selectedNodesStartPositions[_node] = _node.position;
                }

                _leftClickAction?.Invoke(_node, evt.shiftKey, evt.ctrlKey);
                return;
            }
        }

        private void PointerMoveHandler(PointerMoveEvent evt)
        {
            if (_enabled && target.HasPointerCapture(evt.pointerId))
            {
                Vector3 pointerDelta = evt.position - _pointerStartPosition;
                pointerDelta = pointerDelta * (1f / EditorPrefs.GetFloat(LCanvasPrefs.ZOOM_KEY, LCanvasPrefs.DEFAULT_ZOOM));
                Vector2 delta = new Vector2(pointerDelta.x, pointerDelta.y);

                // Move all nodes in the selection
                foreach (var kvp in _selectedNodesStartPositions)
                {
                    var node = kvp.Key;
                    var startPos = kvp.Value;
                    Vector2 newPos = startPos + delta;

                    if (node == _node && EditorPrefs.GetBool(LCanvasPrefs.SNAP_SETTINGS_KEY, true))
                    {
                        newPos = _canvas.TryGetNodeSnapPosition(newPos, _node);
                    }
                    node.SetPosition(newPos);
                }

                // look for groups hover (only for primary node)
                Vector2 primaryNewPos = _node.position;
                if (_canvas.TryGetGroupNodeIsIn(_node, out var groupNodeIsIn))
                {
                    if (_hoveredGroup != null)
                    {
                        _hoveredGroup.SetHoveredWithNode(false);
                        _hoveredGroup = null;
                    }
                }
                else if (_canvas.TryGetGroupAtPosition(primaryNewPos, out var group))
                {
                    if (_hoveredGroup != group && _hoveredGroup != null)
                    {
                        _hoveredGroup.SetHoveredWithNode(false);
                    }
                    _hoveredGroup = group;
                    _hoveredGroup.SetHoveredWithNode(true);
                }
                else
                {
                    if (_hoveredGroup != null)
                    {
                        _hoveredGroup.SetHoveredWithNode(false);
                        _hoveredGroup = null;
                    }
                }
            }
        }

        private void PointerUpHandler(PointerUpEvent evt)
        {
            if (_enabled && target.HasPointerCapture(evt.pointerId))
            {
                _enabled = false;
                target.ReleasePointer(evt.pointerId);
                _node.element.Q("Border").RemoveFromClassList("Pressed");

                if (_hoveredGroup != null)
                {
                    _hoveredGroup.SetHoveredWithNode(false);
                    _canvas.AddNodeToGroupInternal(_node, _hoveredGroup);
                    _hoveredGroup = null;
                }

                // Notify of drag end for all nodes that moved
                foreach (var kvp in _selectedNodesStartPositions)
                {
                    var node = kvp.Key;
                    var startPos = kvp.Value;
                    if (Vector2.Distance(startPos, node.position) > 0.1f)
                    {
                        _dragEndAction?.Invoke(node, startPos);
                    }
                }

                _selectedNodesStartPositions.Clear();
                _isDraggingMultiple = false;
            }
        }

        private void PointerEnterHandler(PointerEnterEvent evt)
        {
            _enterAction.Invoke(_node);
        }

        private void PointerOutHandler(PointerOutEvent evt)
        {
            _exitAction.Invoke(_node);
        }
    }
}

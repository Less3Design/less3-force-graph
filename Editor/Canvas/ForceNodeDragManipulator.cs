using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ForceNodeDragManipulator : PointerManipulator
{
    private bool _enabled;// track if an event started inside the root

    private Vector2 _targetStartPosition { get; set; }
    private Vector3 _pointerStartPosition { get; set; }
    private ForceCanvasNodeElementBase _node;
    private Action<ForceCanvasNodeElementBase> _leftClickAction;
    private Action<ForceCanvasNodeElementBase> _rightClickAction;

    private Action<ForceCanvasNodeElementBase> _enterAction;
    private Action<ForceCanvasNodeElementBase> _exitAction;

    public ForceNodeDragManipulator(
        ForceCanvasNodeElementBase node,
        Action<ForceCanvasNodeElementBase> leftClickAction,
        Action<ForceCanvasNodeElementBase> righClickAction,
        Action<ForceCanvasNodeElementBase> enterAction,
        Action<ForceCanvasNodeElementBase> exitAction)
    {
        _node = node;
        _leftClickAction = leftClickAction;
        _rightClickAction = righClickAction;
        _enterAction = enterAction;
        _exitAction = exitAction;
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
        _targetStartPosition = target.transform.position;
        _pointerStartPosition = evt.position;
        if (evt.button == (int)MouseButton.RightMouse)
        {
            _rightClickAction?.Invoke(_node);
            return;
        }
        if (evt.button == (int)MouseButton.LeftMouse)
        {
            _enabled = true;
            _node.frozen = true;
            PointerCaptureHelper.CapturePointer(target, evt.pointerId);
            _node.element.Q("Border").AddToClassList("Pressed");

            _leftClickAction?.Invoke(_node);
            return;
        }
    }

    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (_enabled && target.HasPointerCapture(evt.pointerId))
        {
            Vector3 pointerDelta = evt.position - _pointerStartPosition;

            _node.SetElementPosition(new Vector2(_targetStartPosition.x + pointerDelta.x, _targetStartPosition.y + pointerDelta.y));
        }
    }

    private void PointerUpHandler(PointerUpEvent evt)
    {
        if (_enabled && target.HasPointerCapture(evt.pointerId))
        {
            _node.frozen = false;
            _enabled = false;
            target.ReleasePointer(evt.pointerId);
            _node.element.Q("Border").RemoveFromClassList("Pressed");
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

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Less3.ForceGraph.Editor
{
    public class ForceDirectedCanvasScrollManipulator : PointerManipulator
    {
        private VisualElement _translationContainer;

        public ForceDirectedCanvasScrollManipulator(VisualElement translationContainer)
        {
            _translationContainer = translationContainer;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<WheelEvent>(WheelHandler);
        }
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<WheelEvent>(WheelHandler);
        }

        private void WheelHandler(WheelEvent evt)
        {
            float delta = evt.delta.y;

            if ((evt.pressedButtons & (1 << 0)) != 0)
            {
                return;
            }

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
}

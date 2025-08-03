using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Less3.ForceGraph.Editor
{
    public class LCanvasGroup<G, N>
    {
        protected readonly Vector2 EMPTY_GROUP_SIZE = new Vector2(100, 100);
        protected readonly Vector2 FILLED_GROUP_PADDING = new Vector2(16, 16);

        public G data;
        public List<LCanvasNode<N>> nodes = new List<LCanvasNode<N>>();
        public VisualElement element;
        public Vector2 position;
        public Label label;

        public bool PositionIsWithinBounds(Vector2 pos)
        {
            Rect bounds = element.worldBound;
            return pos.x >= bounds.xMin && pos.x <= bounds.xMax && pos.y >= bounds.yMin && pos.y <= bounds.yMax;
        }

        public void SetPosition(Vector2 newPos)
        {
            this.position = newPos;
        }

        public void SetHoveredWithNode(bool hovered)
        {
            if (hovered)
            {
                element.Q("Border").AddToClassList("HoveredWithNode");
            }
            else
            {
                element.Q("Border").RemoveFromClassList("HoveredWithNode");
            }
        }

        public LCanvasGroup(G data, VisualElement element, Vector2 position)
        {
            this.data = data;
            this.element = element;
            SetPosition(position);

            label = element.Q<Label>("Label");
            if (data == null)
            {
                label.text = "Group";
            }
            else
            {
                if (data is IForceNodeTitle title)
                    label.text = title.NodeTitle;
                else
                    label.text = data.ToString();
            }
        }

        /// <summary>
        /// When the data on the object has changed. Most likely to change the label
        /// </summary>
        public void UpdateContent()
        {
            if (label == null || data == null)
                return;
            if (data is ILGroupLabel title)
                label.text = title.Label;
            else
                label.text = data.ToString();
        }

        public void UpdateShape()
        {
            Rect rect;

            if (nodes.Count == 0)
            {
                rect = new Rect(position, EMPTY_GROUP_SIZE);
            }
            else
            {
                rect = new Rect(nodes[0].element.localBound);
                for (int i = 1; i < nodes.Count; i++)
                {
                    LCanvasNode<N> node = nodes[i];
                    if (node == null || node.element == null)
                        continue;
                    rect = rect.Encapsulate(node.element.localBound);
                }
                // add padding
                rect.xMin -= FILLED_GROUP_PADDING.x;
                rect.xMax += FILLED_GROUP_PADDING.x;
                rect.yMin -= FILLED_GROUP_PADDING.y;
                rect.yMax += FILLED_GROUP_PADDING.y;
            }
            element.style.left = rect.x;
            element.style.top = rect.y;
            element.style.width = rect.width;
            element.style.height = rect.height;
        }

        public void AddNode(LCanvasNode<N> node)
        {
            nodes.Add(node);
        }

        public void RemoveNode(LCanvasNode<N> node)
        {
            nodes.Remove(node);
        }
    }
}

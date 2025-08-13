using UnityEngine;
using UnityEngine.UIElements;

namespace Less3.ForceGraph.Editor
{
    public class LCanvasNode<N>
    {
        public VisualElement element;
        public Vector2 position { get; protected set; }

        public Rect bounds => element.worldBound;

        public void SetPosition(Vector2 newPos)
        {
            this.position = newPos;
            element.transform.position = newPos;
        }

        private N _data;
        public N data
        {
            get => _data;
            set
            {
                _data = value;
                if (_data != null)
                {
                    if (_data is IForceNodeTitle title)
                        element.Q<Label>("Label").text = title.NodeTitle;
                    else
                        element.Q<Label>("Label").text = value.ToString();

                    Label surTitleLabel = element.Q<Label>("SurLabel");
                    if (_data is IForceNodeSurTitle surTitle && !string.IsNullOrEmpty(surTitle.NodeSurTitle))
                    {
                        surTitleLabel.text = surTitle.NodeSurTitle;
                        surTitleLabel.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        surTitleLabel.style.display = DisplayStyle.None;
                    }

                    var icon = element.Q<VisualElement>("Icon");
                    if (_data is IForceNodeStyle style)
                    {
                        element.Q("NodeContainer").style.backgroundColor = style.NodeBackgroundColor;
                        element.Q<Label>("Label").style.color = style.NodeLabelColor;
                        icon.style.unityBackgroundImageTintColor = style.NodeLabelColor;
                    }
                    if (_data is IForceNodeScale scale)
                    {
                        element.transform.scale = new Vector3(scale.NodeScale, scale.NodeScale, 1f);
                    }
                    if (_data is IForceNodeIcon iconData)
                    {
                        icon.style.display = DisplayStyle.Flex;
                        icon.style.backgroundImage = Resources.Load<Texture2D>(iconData.NodeIcon);
                    }
                    else
                    {
                        icon.style.display = DisplayStyle.None;
                    }
                }
            }
        }

        public LCanvasNode(N data, VisualElement element, Vector2 newPos)
        {
            this.element = element;
            this.data = data;
            position = newPos;
            element.transform.position = newPos;
        }

        public void UpdateContent()
        {
            data = data;// cheeky way to trigger the setter and update the UI.
        }
    }
}

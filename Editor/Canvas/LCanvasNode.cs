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

                    // Badges
                    if (_data is IForceNodeBadges badges)
                    {
                        element.Q<VisualElement>("Badges").style.display = badges.NodeBadges != NodeBadges.None ? DisplayStyle.Flex : DisplayStyle.None;
                        element.Q<VisualElement>("tip").style.display = (badges.NodeBadges & NodeBadges.Tip) != 0 ? DisplayStyle.Flex : DisplayStyle.None;
                        element.Q<VisualElement>("info").style.display = (badges.NodeBadges & NodeBadges.Info) != 0 ? DisplayStyle.Flex : DisplayStyle.None;
                        element.Q<VisualElement>("warning").style.display = (badges.NodeBadges & NodeBadges.Warning) != 0 ? DisplayStyle.Flex : DisplayStyle.None;
                        element.Q<VisualElement>("error").style.display = (badges.NodeBadges & NodeBadges.Error) != 0 ? DisplayStyle.Flex : DisplayStyle.None;
                    }
                    else
                    {
                        element.Q<VisualElement>("Badges").style.display = DisplayStyle.None;
                    }

                    var icon = element.Q<VisualElement>("Icon");
                    if (_data is IForceNodeStyle style)
                    {
                        element.Q("NodeContainer").style.backgroundColor = style.NodeBackgroundColor;
                        element.Q<Label>("Label").style.color = style.NodeLabelColor;
                        surTitleLabel.style.color = style.NodeLabelColor;
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

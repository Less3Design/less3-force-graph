using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Less3.ForceGraph.Editor
{
    public class LCanvasNode<N>
    {
        public VisualElement element;
        public Vector2 position { get; protected set; }

        // we dynamically create tags.
        // Update can get called often, so we don't want to constantly create/destroy them.
        private List<Label> _tagPool = new List<Label>();

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
                    {
                        if (string.IsNullOrEmpty(title.NodeTitle))
                        {
                            element.Q<Label>("Label").style.display = DisplayStyle.None;
                        }
                        else
                        {
                            element.Q<Label>("Label").style.display = DisplayStyle.Flex;
                            element.Q<Label>("Label").text = title.NodeTitle;
                        }
                    }
                    else
                    {
                        element.Q<Label>("Label").text = value.ToString();
                    }

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

                    VisualElement tagContainer = element.Q<VisualElement>("tags");

                    if (_data is ILCanvasTags subTitle)
                    {
                        int c = 0;
                        foreach (var tag in subTitle.NodeTags)
                        {
                            Label l;
                            if (c < _tagPool.Count)
                            {
                                l = _tagPool[c];
                                l.style.display = DisplayStyle.Flex;
                            }
                            else
                            {
                                l = new Label();
                                l.AddToClassList("Tag");
                                l.pickingMode = PickingMode.Ignore;
                                tagContainer.Add(l);
                                _tagPool.Add(l);
                            }
                            l.text = tag.text;
                            l.tooltip = tag.tooltip;
                            c++;
                        }
                        for (int i = c; i < _tagPool.Count; i++)
                        {
                            _tagPool[i].style.display = DisplayStyle.None;
                        }
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
                    else
                    {
                        element.Q("NodeContainer").style.backgroundColor = ForceNode.defaultBackgroundColor;
                        element.Q<Label>("Label").style.color = ForceNode.defaultTextColor;
                        surTitleLabel.style.color = ForceNode.defaultTextColor;
                        icon.style.unityBackgroundImageTintColor = ForceNode.defaultTextColor;
                    }

                    if (_data is IForceNodeScale scale)
                    {
                        element.transform.scale = new Vector3(scale.NodeScale, scale.NodeScale, 1f);
                    }

                    if (_data is IForceNodeIcon iconData && !string.IsNullOrEmpty(iconData.NodeIcon))
                    {
                        Texture2D tex = Resources.Load<Texture2D>(iconData.NodeIcon);
                        if (tex == null)
                        {
                            icon.style.display = DisplayStyle.None;
                        }
                        else
                        {
                            icon.style.display = DisplayStyle.Flex;
                            icon.style.backgroundImage = Resources.Load<Texture2D>(iconData.NodeIcon);
                        }
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
            data = data;
        }
    }
}

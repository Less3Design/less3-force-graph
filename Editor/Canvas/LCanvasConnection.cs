using UnityEngine;
using UnityEngine.UIElements;

namespace Less3.ForceGraph.Editor
{
    public class LCanvasConnection<N, C>
    {
        public static readonly string CONNECTION_DASH_TEXTURE = "TiledConnection";
        private C _data;
        public C data
        {
            get => _data;
            set
            {
                _data = value;
                UpdateVisuals();
            }
        }

        private VisualElement _element;
        public VisualElement element
        {
            get => _element;
            set
            {
                _element = value;
                UpdateVisuals();
            }
        }

        public LCanvasNode<N> from;
        public LCanvasNode<N> to;

        private void UpdateVisuals()
        {
            // so messy
            if (_element != null && _data != null)
            {
                VisualElement arrow = _element.Q("DirArrow");
                if (_data is IForceConnectionStyle style)
                {
                    arrow.style.unityBackgroundImageTintColor = style.ConnectionColor;
                    if (style.Dashed)
                    {
                        _element.style.backgroundImage = Background.FromTexture2D(Resources.Load<Texture2D>(CONNECTION_DASH_TEXTURE));
                        _element.style.unityBackgroundImageTintColor = style.ConnectionColor;
                        _element.style.backgroundColor = Color.clear;
                        _element.style.backgroundRepeat = new BackgroundRepeat(Repeat.Repeat, Repeat.Repeat);
                        _element.style.backgroundSize = new BackgroundSize(Length.Auto(), Length.Auto());
                    }
                    else
                    {
                        _element.style.backgroundImage = null;
                        _element.style.unityBackgroundImageTintColor = ForceConnection.defaultColor;
                        _element.style.backgroundColor = style.ConnectionColor;
                    }

                    arrow.style.unityBackgroundImageTintColor = _element.style.backgroundColor;
                }
                else
                {
                    _element.style.backgroundColor = ForceConnection.defaultColor;
                    arrow.style.unityBackgroundImageTintColor = ForceConnection.defaultColor;
                    _element.style.unityBackgroundImageTintColor = Color.clear;
                }

                if (_data is IForceConnectionIsDirectional directional)
                {
                    arrow.style.display = directional.IsDirectional ? DisplayStyle.Flex : DisplayStyle.None;
                }
                else
                {
                    arrow.style.display = DisplayStyle.None;
                }
            }
        }

        public void UpdateContent()
        {
            data = data;
        }
    }
}

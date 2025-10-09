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
            if (_element != null && _data != null && _data is IForceConnectionStyle style)
            {
                if (style.Dashed)
                {
                    _element.style.backgroundImage = Background.FromTexture2D(Resources.Load<Texture2D>(CONNECTION_DASH_TEXTURE));
                    _element.style.unityBackgroundImageTintColor = style.ConnectionColor;
                    _element.style.backgroundColor = Color.clear;
                    _element.style.backgroundRepeat = new BackgroundRepeat(Repeat.Repeat, Repeat.Repeat);
                    _element.style.backgroundSize = new BackgroundSize(Length.Auto(), Length.Auto());

                    if (_data is IForceConnectionIsDirectional dir)
                    {
                        VisualElement arrow = _element.Q("DirArrow");
                        arrow.style.display = dir.IsDirectional ? DisplayStyle.Flex : DisplayStyle.None;
                        arrow.style.unityBackgroundImageTintColor = style.ConnectionColor;
                    }
                    else
                    {
                        _element.Q("DirArrow").style.display = DisplayStyle.None;
                    }
                }
                else
                {
                    _element.style.backgroundImage = null;
                    _element.style.unityBackgroundImageTintColor = ForceConnection.defaultColor;
                    _element.style.backgroundColor = style.ConnectionColor;

                    if (_data is IForceConnectionIsDirectional dir)
                    {
                        VisualElement arrow = _element.Q("DirArrow");
                        arrow.style.display = dir.IsDirectional ? DisplayStyle.Flex : DisplayStyle.None;
                        arrow.style.unityBackgroundImageTintColor = style.ConnectionColor;
                    }
                    else
                    {
                        _element.Q("DirArrow").style.display = DisplayStyle.None;
                    }
                }
            }
        }

        public void UpdateContent()
        {
            data = data;
        }
    }
}

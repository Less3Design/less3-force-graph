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
                if (_data != null)
                {
                    if (_data is IForceConnectionStyle style && _element != null)
                    {
                        if (style.Dashed)
                        {
                            _element.style.backgroundImage = Background.FromTexture2D(Resources.Load<Texture2D>(CONNECTION_DASH_TEXTURE));
                            _element.style.unityBackgroundImageTintColor = style.ConnectionColor;
                            _element.style.backgroundColor = Color.clear;
                            _element.style.backgroundRepeat = new BackgroundRepeat(Repeat.Repeat, Repeat.NoRepeat);
                            _element.style.backgroundSize = new BackgroundSize(Length.Auto(), Length.Auto());
                        }
                        else
                        {
                            _element.style.backgroundColor = style.ConnectionColor;
                        }
                    }
                }
            }
        }

        private VisualElement _element;
        public VisualElement element
        {
            get => _element;
            set
            {
                _element = value;
                if (_data != null && _data is IForceConnectionStyle style)
                {
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
                        _element.style.backgroundColor = style.ConnectionColor;
                    }
                }
            }
        }
        public LCanvasNode<N> from;
        public LCanvasNode<N> to;
    }
}

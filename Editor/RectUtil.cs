using UnityEngine;

namespace Less3.ForceGraph.Editor
{
    public static class RectUtil//rect extension
    {
        public static Rect Encapsulate(this Rect rect, Rect other)
        {
            float minX = Mathf.Min(rect.xMin, other.xMin);
            float minY = Mathf.Min(rect.yMin, other.yMin);
            float maxX = Mathf.Max(rect.xMax, other.xMax);
            float maxY = Mathf.Max(rect.yMax, other.yMax);
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
    }
}

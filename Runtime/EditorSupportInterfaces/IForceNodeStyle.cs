using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Less3.ForceGraph
{
    /// <summary>
    /// Interface you can implement on a type to set its styles for use in the force directed canvas.
    /// </summary>
    public interface IForceNodeStyle
    {
        //public Texture2D NodeIcon { get; } //TODO
        public Color NodeBackgroundColor { get; }
        public Color NodeLabelColor { get; }
    }
}

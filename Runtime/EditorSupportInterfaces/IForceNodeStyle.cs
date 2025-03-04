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
        public Color NodeBackgroundColor { get; }
        public Color NodeLabelColor { get; }
    }

    /// <summary>
    /// Override the title of a node in the editor canvas. If unused the node is named using `ToString()` on the node data
    /// </summary>
    public interface IForceNodeTitle
    {
        public string NodeTitle { get; }
    }

    /// <summary>
    /// Set the icon that appears on the node in the editor canvas. Icon is pulled from resources by name
    /// </summary>
    public interface IForceNodeIcon
    {
        public string NodeIcon { get; }
    }

    /// <summary>
    /// Set the scale of the node in the editor canvas. If unused the node is scaled to 1
    /// </summary>
    public interface IForceNodeScale
    {
        public float NodeScale { get; }
    }

    /// <summary>
    /// Set the color of a connection in the force graph editor canvas
    /// </summary>
    public interface IForceConnectionStyle
    {
        public Color ConnectionColor { get; }
        public bool Dashed { get; }
    }
}

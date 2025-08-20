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

    public interface IForceNodeSurTitle
    {
        public string NodeSurTitle { get; }
    }

    public interface IForceNodeSubTitle
    {
        public string NodeSubTitle { get; }
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

    public interface IForceNodeBadges
    {
        public NodeBadges NodeBadges { get; }
    }

    [System.Flags]
    public enum NodeBadges
    {
        None = 0,
        Tip = 1 << 0, // A green-ish square
        Info = 1 << 1, // A white circle
        Warning = 1 << 2, // A yellow triangle
        Error = 1 << 3, // A red octagon
    }

    public interface ILNodeEditorDoubleClick
    {
        /// <summary>
        /// Called when the node is double clicked in the editor canvas.
        /// </summary>
        void EditorOnNodeDoubleClick();
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

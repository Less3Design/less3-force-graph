using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Less3.ForceGraph
{
    public abstract class ForceNode : ScriptableObject
    {
        /// <summary>
        /// The position of the node in the graph.
        /// </summary>
        [HideInInspector]
        public Vector2 position;
        [HideInInspector]
        public ForceGraph graph { get; private set; }

        public void SetGraph(ForceGraph graph)
        {
            if (this.graph != null)
            {
                Debug.LogError("You cannot change the graph of a node after creation.");
                return;
            }
            this.graph = graph;
        }

        override public string ToString()
        {
            return name;
        }

        public static Color defaultBackgroundColor = new Color(0.234f, .234f, .234f, 1f);
        public static Color defaultTextColor = new Color(.82f, .82f, .82f, 1f);
    }
}

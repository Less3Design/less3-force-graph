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
        public Vector2 position;
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
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Less3.Graph
{
    public abstract class L3GraphGroup : ScriptableObject 
    {
        /// <summary>
        /// Position only used if the group is empty. Otherwise position/scale is derived from the nodes.
        /// </summary>
        [HideInInspector]
        public Vector2 position;
        public L3Graph graph { get; private set; }
        [HideInInspector]
        public List<L3GraphNode> nodes = new List<L3GraphNode>();

        public void SetGraph(L3Graph graph)
        {
            if (this.graph != null)
            {
                Debug.LogError("You cannot change the graph of a group after creation.");
                return;
            }
            this.graph = graph;
        }
    }
}

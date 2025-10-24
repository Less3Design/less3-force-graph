using System.Collections.Generic;
using UnityEngine;

namespace Less3.ForceGraph
{
    public abstract class ForceGroup : ScriptableObject 
    {
        /// <summary>
        /// Position only used if the group is empty. Otherwise position/scale is derived from the nodes.
        /// </summary>
        [HideInInspector]
        public Vector2 position;
        public ForceGraph graph { get; private set; }
        [HideInInspector]
        public List<ForceNode> nodes = new List<ForceNode>();

        public void SetGraph(ForceGraph graph)
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

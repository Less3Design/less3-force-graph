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

        override public string ToString()
        {
            return name;
        }
    }
}

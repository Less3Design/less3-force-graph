using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Less3.ForceGraph
{
    public abstract class ForceGraph : ScriptableObject
    {
        public List<ForceNode> nodes = new List<ForceNode>();
        public List<ForceConnection> connections = new List<ForceConnection>();

        private void OnEnable()
        {
            //Validation. 
            foreach(ForceNode n in nodes)
            {
                if (n.graph == null)
                {
                    n.SetGraph(this);
                }
            }
        }

        /// <summary>
        /// Determine if a connection can be made between two nodes. By default this just simply checks if a connection already exists between the two nodes.
        /// </summary>
        public virtual bool ValidateConnectionRequest(ForceNode from, ForceNode to, Type connectionType)
        {
            // check if any connections already exist between these nodes
            foreach (var connection in connections)
            {
                if (connection.from == from && connection.to == to)
                {
                    return false;
                }
                if (connection.from == to && connection.to == from)
                {
                    return false;
                }
            }
            return true;
        }

        public abstract List<(string, Type)> GraphNodeTypes();
        /// <summary>
        /// Returns a dictionary that lists all the connection types that can be made from a node type
        /// Dict<{NodeType, List<(ConnectionName, ConnectionType)>}>
        /// </summary>
        public abstract Dictionary<Type, List<(string, Type)>> GraphConnectionTypes();
    }
}

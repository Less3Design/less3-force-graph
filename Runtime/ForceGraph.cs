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
        public List<ForceGroup> groups = new List<ForceGroup>();

        private void OnEnable()
        {
            //Validation. This excists because Nodes didnt have a graph reference before. It should be unnecessary for new graphs.
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                ForceNode n = nodes[i];
                if (n == null)
                {
                    nodes.RemoveAt(i);
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(this);
#endif
                    continue;
                }
                if (n.graph == null)
                {
                    n.SetGraph(this);
                }
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(n);
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }

            for (int i = groups.Count - 1; i >= 0; i--)
            {
                ForceGroup g = groups[i];
                if (g == null)
                {
                    groups.RemoveAt(i);
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(this);
#endif
                    continue;
                }
                if (g.graph == null)
                {
                    g.SetGraph(this);

#if UNITY_EDITOR

                    UnityEditor.EditorUtility.SetDirty(g);
                    UnityEditor.EditorUtility.SetDirty(this);
#endif
                }
            }
        }

        public abstract List<(string, Type)> GraphNodeTypes();
        public abstract List<(string, Type)> GraphGroupTypes();
        /// <summary>
        /// Returns a dictionary that lists all the connection types that can be made from a node type
        /// Dict<{NodeType, List<(ConnectionName, ConnectionType)>}>
        /// Used to populate right click menu on nodes.
        /// </summary>
        public abstract Dictionary<Type, List<(string, Type)>> GraphConnectionTypes();

        /// <summary>
        /// Determine if a connection can be made between two nodes.
        /// By default this just simply checks if a connection already exists between the two nodes.
        /// Each graph type should probably override this with custom rules.
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

        public virtual Type AutoConnnectionRequest(ForceNode from, ForceNode to)
        {
            if (GraphConnectionTypes().TryGetValue(from.GetType(), out var connectionTypes) && connectionTypes.Count > 0)
            {
                Type t = connectionTypes[0].Item2;
                if (ValidateConnectionRequest(from, to, t))
                {
                    return t;
                }
            }

            return null;
        }

        // ***********************
        // *  Utility Functions  *
        // ***********************

        public List<ForceConnection> GetNodeConnections(ForceNode node)
        {
            List<ForceConnection> foundConnections = new List<ForceConnection>();
            foreach (ForceConnection connection in connections)
            {
                if (connection.from == node || connection.to == node)
                {
                    foundConnections.Add(connection);
                }
            }
            return foundConnections;
        }

        public List<T> GetConnectionsOfType<T>() where T : ForceConnection
        {
            List<T> foundConnections = new List<T>();
            foreach (ForceConnection connection in connections)
            {
                if (connection is T)
                {
                    foundConnections.Add(connection as T);
                }
            }
            return foundConnections;
        }

        public List<T> GetNodeConnectionsOfType<T>(ForceNode node) where T : ForceConnection
        {
            List<T> foundConnections = new List<T>();

            foreach (ForceConnection connection in GetNodeConnections(node))
            {
                if (connection is T)
                {
                    foundConnections.Add(connection as T);
                }
            }
            return foundConnections;
        }

        public List<T> GetNodesOfType<T>() where T : ForceNode
        {
            List<T> foundNodes = new List<T>();
            foreach (ForceNode node in nodes)
            {
                if (node is T)
                {
                    foundNodes.Add(node as T);
                }
            }
            return foundNodes;
        }

        public List<T> GetGroupsOfType<T>() where T : ForceGroup
        {
            List<T> foundGroups = new List<T>();
            foreach (ForceGroup group in groups)
            {
                if (group is T)
                {
                    foundGroups.Add(group as T);
                }
            }
            return foundGroups;
        }

        /// <summary>
        /// If the node is part of a group, returns that group.
        /// </summary>
        public bool TryGetNodeGroup(ForceNode node, out ForceGroup group)
        {
            group = null;
            foreach (ForceGroup g in groups)
            {
                if (g.nodes.Contains(node))
                {
                    group = g;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns all nodes that are not the `upstreamNode` and are connected to the `node`.
        /// </summary>
        public List<ForceNode> GetDownstreamNodes(ForceNode node, ForceNode upstreamNode = null)
        {
            List<ForceNode> downstreamNodes = new List<ForceNode>();
            foreach (ForceConnection connection in GetNodeConnections(node))
            {
                if (connection.from == node && connection.to != upstreamNode)
                {
                    downstreamNodes.Add(connection.to);
                }
                else if (connection.to == node && connection.from != upstreamNode)
                {
                    downstreamNodes.Add(connection.from);
                }
            }
            return downstreamNodes;
        }

        public List<T> GetDownstreamNodes<T>(ForceNode node, ForceNode upstreamNode = null) where T : ForceNode
        {
            List<T> downstreamNodes = new List<T>();
            foreach (ForceConnection connection in GetNodeConnections(node))
            {
                if (connection.from == node && connection.to != upstreamNode && connection.to is T castedTo)
                {
                    downstreamNodes.Add(castedTo);
                }
                else if (connection.to == node && connection.from != upstreamNode && connection.from is T castedFrom)
                {
                    downstreamNodes.Add(castedFrom);
                }
            }
            return downstreamNodes;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Less3.ForceGraph.Editor
{
    public static class ForceGraphEditorUtil
    {
        public static ForceNode CreateNode(this ForceGraph graph, Type type)
        {
            var node = (ForceNode)ScriptableObject.CreateInstance(type);
            node.name = type.Name;
            AssetDatabase.AddObjectToAsset(node, graph);
            node.SetGraph(graph);
            graph.nodes.Add(node);
            return node;
        }

        public static T CreateNode<T>(this ForceGraph graph) where T : ForceNode
        {
            var newNode = ScriptableObject.CreateInstance<T>();
            newNode.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(newNode, graph);
            newNode.SetGraph(graph);
            graph.nodes.Add(newNode);
            return newNode;
        }

        public static ForceNode DuplicateNode(this ForceGraph graph, ForceNode node)
        {
            var newNode = ScriptableObject.Instantiate(node);
            newNode.name = node.name;
            AssetDatabase.AddObjectToAsset(newNode, graph);
            newNode.SetGraph(graph);
            graph.nodes.Add(newNode);
            newNode.position = node.position + Vector2.up * 25f;
            return newNode;
        }

        public static void DeleteNode(this ForceGraph graph, ForceNode node)
        {
            if (graph.nodes.Contains(node) == false)
            {
                Debug.LogError("Cannot delete node that is not in the graph");
                return;
            }
            graph.nodes.Remove(node);
            ScriptableObject.DestroyImmediate(node, true);
        }

        public static ForceConnection CreateConnection(this ForceGraph graph, ForceNode from, ForceNode to, Type type)
        {
            if (graph.nodes.Contains(from) == false || graph.nodes.Contains(to) == false)
            {
                Debug.LogError("Cannot create connection between nodes that are not in the graph");
                return null;
            }
            if (graph.ValidateConnectionRequest(from, to, type) == false)
            {
                return null;
            }

            //TODO: validate if `type` is a child of ForceConnection

            var newConnection = (ForceConnection)ScriptableObject.CreateInstance(type);
            newConnection.name = type.Name;
            newConnection.from = from;
            newConnection.to = to;
            AssetDatabase.AddObjectToAsset(newConnection, graph);
            graph.connections.Add(newConnection);
            return newConnection;
        }

        public static void DeleteConnection(this ForceGraph graph, ForceConnection connection)
        {
            if (graph.connections.Contains(connection) == false)
            {
                Debug.LogError("Cannot delete connection that is not in the graph");
                return;
            }
            graph.connections.Remove(connection);
            ScriptableObject.DestroyImmediate(connection, true);
        }

        public static ForceGroup CreateGroup(this ForceGraph graph, Type type)
        {
            var group = (ForceGroup)ScriptableObject.CreateInstance(type);
            group.name = type.Name;
            AssetDatabase.AddObjectToAsset(group, graph);
            group.SetGraph(graph);
            graph.groups.Add(group);
            return group;
        }

        public static void AddNodeToGroup(this ForceGraph graph, ForceNode node, ForceGroup group)
        {
            if (group == null || node == null)
            {
                return;
            }

            RemoveNodeFromGroups(graph, node);
            if (group.nodes.Contains(node) == false)
            {
                group.nodes.Add(node);
            }
        }

        /// <summary>
        /// Remove the node from any groups it is in.
        /// </summary>
        public static void RemoveNodeFromGroups(this ForceGraph graph, ForceNode node)
        {
            foreach (var group in graph.groups)
            {
                if (group.nodes.Contains(node))
                {
                    group.nodes.Remove(node);
                }
            }
        }

        public static void IsNodeInAGroup(this ForceGraph graph, ForceNode node, out ForceGroup group)
        {
            group = null;
            foreach (var g in graph.groups)
            {
                if (g.nodes.Contains(node))
                {
                    group = g;
                    return;
                }
            }
        }
    }
}

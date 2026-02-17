using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Less3.Graph.Editor
{
    public static class ForceGraphEditorUtil
    {
        public static L3GraphNode CreateNode(this L3Graph graph, Type type)
        {
            var node = (L3GraphNode)ScriptableObject.CreateInstance(type);
            node.name = type.Name;
            AssetDatabase.AddObjectToAsset(node, graph);
            node.SetGraph(graph);
            graph.nodes.Add(node);
            EditorUtility.SetDirty(graph);
            return node;
        }

        public static T CreateNode<T>(this L3Graph graph) where T : L3GraphNode
        {
            var newNode = ScriptableObject.CreateInstance<T>();
            newNode.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(newNode, graph);
            newNode.SetGraph(graph);
            graph.nodes.Add(newNode);
            EditorUtility.SetDirty(graph);
            return newNode;
        }

        public static L3GraphNode DuplicateNode(this L3Graph graph, L3GraphNode node)
        {
            var newNode = ScriptableObject.Instantiate(node);
            newNode.name = node.name;
            AssetDatabase.AddObjectToAsset(newNode, graph);
            newNode.SetGraph(graph);
            graph.nodes.Add(newNode);
            newNode.position = node.position + Vector2.up * 25f;
            EditorUtility.SetDirty(graph);
            return newNode;
        }

        public static void DeleteNode(this L3Graph graph, L3GraphNode node)
        {
            if (graph.nodes.Contains(node) == false)
            {
                Debug.LogError("Cannot delete node that is not in the graph");
                return;
            }
            graph.nodes.Remove(node);
            ScriptableObject.DestroyImmediate(node, true);
            EditorUtility.SetDirty(graph);
        }

        public static L3GraphConnection CreateConnection(this L3Graph graph, L3GraphNode from, L3GraphNode to, Type type)
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

            var newConnection = (L3GraphConnection)ScriptableObject.CreateInstance(type);
            newConnection.name = type.Name;
            newConnection.from = from;
            newConnection.to = to;
            AssetDatabase.AddObjectToAsset(newConnection, graph);
            graph.connections.Add(newConnection);
            EditorUtility.SetDirty(graph);
            return newConnection;
        }

        public static void DeleteConnection(this L3Graph graph, L3GraphConnection connection)
        {
            if (graph.connections.Contains(connection) == false)
            {
                Debug.LogError("Cannot delete connection that is not in the graph");
                return;
            }
            graph.connections.Remove(connection);
            ScriptableObject.DestroyImmediate(connection, true);
            EditorUtility.SetDirty(graph);
        }

        public static L3GraphGroup CreateGroup(this L3Graph graph, Type type)
        {
            var group = (L3GraphGroup)ScriptableObject.CreateInstance(type);
            group.name = type.Name;
            AssetDatabase.AddObjectToAsset(group, graph);
            group.SetGraph(graph);
            graph.groups.Add(group);
            EditorUtility.SetDirty(graph);
            return group;
        }

        public static void AddNodeToGroup(this L3Graph graph, L3GraphNode node, L3GraphGroup group)
        {
            if (group == null || node == null)
            {
                return;
            }

            RemoveNodeFromAllGroups(graph, node);
            if (group.nodes.Contains(node) == false)
            {
                group.nodes.Add(node);
            }

            EditorUtility.SetDirty(group);
        }

        /// <summary>
        /// Remove the node from any groups it is in.
        /// </summary>
        public static void RemoveNodeFromAllGroups(this L3Graph graph, L3GraphNode node)
        {
            foreach (var group in graph.groups)
            {
                if (group.nodes.Contains(node))
                {
                    group.nodes.Remove(node);
                    EditorUtility.SetDirty(group);
                }
            }
        }

        /// <summary>
        ///  Currently assuming that group relationship is one way. Nodes should not know about their groups without querying the graph.
        /// </summary>
        public static void DeleteGroup(this L3Graph graph, L3GraphGroup group)
        {
            if (graph.groups.Contains(group) == false)
            {
                Debug.LogError("Cannot delete group that is not in the graph");
                return;
            }
            graph.groups.Remove(group);
            ScriptableObject.DestroyImmediate(group, true);
            EditorUtility.SetDirty(graph);
        }

        public static void IsNodeInAGroup(this L3Graph graph, L3GraphNode node, out L3GraphGroup group)
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

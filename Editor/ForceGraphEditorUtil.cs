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
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssetIfDirty(graph);
            return node;
        }

        public static T CreateNode<T>(this ForceGraph graph) where T : ForceNode
        {
            var newNode = ScriptableObject.CreateInstance<T>();
            newNode.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(newNode, graph);
            newNode.SetGraph(graph);
            graph.nodes.Add(newNode);
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssetIfDirty(graph);
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
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssetIfDirty(graph);
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
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssetIfDirty(graph);
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
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssetIfDirty(graph);
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
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssetIfDirty(graph);
        }

        public static ForceGroup CreateGroup(this ForceGraph graph, Type type)
        {
            var group = (ForceGroup)ScriptableObject.CreateInstance(type);
            group.name = type.Name;
            AssetDatabase.AddObjectToAsset(group, graph);
            group.SetGraph(graph);
            graph.groups.Add(group);
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssetIfDirty(graph);
            return group;
        }

        public static void AddNodeToGroup(this ForceGraph graph, ForceNode node, ForceGroup group)
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
            AssetDatabase.SaveAssetIfDirty(group);
        }

        /// <summary>
        /// Remove the node from any groups it is in.
        /// </summary>
        public static void RemoveNodeFromAllGroups(this ForceGraph graph, ForceNode node)
        {
            foreach (var group in graph.groups)
            {
                if (group.nodes.Contains(node))
                {
                    group.nodes.Remove(node);
                    EditorUtility.SetDirty(group);
                    AssetDatabase.SaveAssetIfDirty(group);
                }
            }
            AssetDatabase.SaveAssetIfDirty(graph);
        }

        /// <summary>
        ///  Currently assuming that group relationship is one way. Nodes should not know about their groups without querying the graph.
        /// </summary>
        public static void DeleteGroup(this ForceGraph graph, ForceGroup group)
        {
            if (graph.groups.Contains(group) == false)
            {
                Debug.LogError("Cannot delete group that is not in the graph");
                return;
            }
            graph.groups.Remove(group);
            ScriptableObject.DestroyImmediate(group, true);
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssetIfDirty(graph);
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

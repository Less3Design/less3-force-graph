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
            Undo.RecordObject(graph, "Create Node");
            var node = (ForceNode)ScriptableObject.CreateInstance(type);
            node.name = type.Name;
            Undo.RegisterCreatedObjectUndo(node, "Create Node");
            AssetDatabase.AddObjectToAsset(node, graph);
            node.SetGraph(graph);
            graph.nodes.Add(node);
            EditorUtility.SetDirty(graph);
            return node;
        }

        public static T CreateNode<T>(this ForceGraph graph) where T : ForceNode
        {
            Undo.RecordObject(graph, "Create Node");
            var newNode = ScriptableObject.CreateInstance<T>();
            newNode.name = typeof(T).Name;
            Undo.RegisterCreatedObjectUndo(newNode, "Create Node");
            AssetDatabase.AddObjectToAsset(newNode, graph);
            newNode.SetGraph(graph);
            graph.nodes.Add(newNode);
            EditorUtility.SetDirty(graph);
            return newNode;
        }

        public static ForceNode DuplicateNode(this ForceGraph graph, ForceNode node)
        {
            Undo.RecordObject(graph, "Duplicate Node");
            var newNode = ScriptableObject.Instantiate(node);
            newNode.name = node.name;
            Undo.RegisterCreatedObjectUndo(newNode, "Duplicate Node");
            AssetDatabase.AddObjectToAsset(newNode, graph);
            newNode.SetGraph(graph);
            graph.nodes.Add(newNode);
            newNode.position = node.position + Vector2.up * 25f;
            EditorUtility.SetDirty(graph);
            return newNode;
        }

        public static void DeleteNode(this ForceGraph graph, ForceNode node)
        {
            if (graph.nodes.Contains(node) == false)
            {
                Debug.LogError("Cannot delete node that is not in the graph");
                return;
            }
            Undo.RecordObject(graph, "Delete Node");
            graph.nodes.Remove(node);
            Undo.DestroyObjectImmediate(node);
            EditorUtility.SetDirty(graph);
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

            Undo.RecordObject(graph, "Create Connection");
            var newConnection = (ForceConnection)ScriptableObject.CreateInstance(type);
            newConnection.name = type.Name;
            newConnection.from = from;
            newConnection.to = to;
            Undo.RegisterCreatedObjectUndo(newConnection, "Create Connection");
            AssetDatabase.AddObjectToAsset(newConnection, graph);
            graph.connections.Add(newConnection);
            EditorUtility.SetDirty(graph);
            return newConnection;
        }

        public static void DeleteConnection(this ForceGraph graph, ForceConnection connection)
        {
            if (graph.connections.Contains(connection) == false)
            {
                Debug.LogError("Cannot delete connection that is not in the graph");
                return;
            }
            Undo.RecordObject(graph, "Delete Connection");
            graph.connections.Remove(connection);
            Undo.DestroyObjectImmediate(connection);
            EditorUtility.SetDirty(graph);
        }

        public static ForceGroup CreateGroup(this ForceGraph graph, Type type)
        {
            Undo.RecordObject(graph, "Create Group");
            var group = (ForceGroup)ScriptableObject.CreateInstance(type);
            group.name = type.Name;
            Undo.RegisterCreatedObjectUndo(group, "Create Group");
            AssetDatabase.AddObjectToAsset(group, graph);
            group.SetGraph(graph);
            graph.groups.Add(group);
            EditorUtility.SetDirty(graph);
            return group;
        }

        public static void AddNodeToGroup(this ForceGraph graph, ForceNode node, ForceGroup group)
        {
            if (group == null || node == null)
            {
                return;
            }

            RemoveNodeFromAllGroups(graph, node);
            Undo.RecordObject(group, "Add Node to Group");
            if (group.nodes.Contains(node) == false)
            {
                group.nodes.Add(node);
            }

            EditorUtility.SetDirty(group);
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
                    Undo.RecordObject(group, "Remove Node from Group");
                    group.nodes.Remove(node);
                    EditorUtility.SetDirty(group);
                }
            }
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
            Undo.RecordObject(graph, "Delete Group");
            graph.groups.Remove(group);
            Undo.DestroyObjectImmediate(group);
            EditorUtility.SetDirty(graph);
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

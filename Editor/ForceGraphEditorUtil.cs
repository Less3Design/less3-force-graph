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
#if UNDO_EXPERIMENTAL
            Undo.RecordObject(graph, "Create Node");
#endif
            var node = (ForceNode)ScriptableObject.CreateInstance(type);
            node.name = type.Name;
#if UNDO_EXPERIMENTAL
            Undo.RegisterCreatedObjectUndo(node, "Create Node");
#endif
            AssetDatabase.AddObjectToAsset(node, graph);
            node.SetGraph(graph);
            graph.nodes.Add(node);
            EditorUtility.SetDirty(graph);
            return node;
        }

        public static T CreateNode<T>(this ForceGraph graph) where T : ForceNode
        {
#if UNDO_EXPERIMENTAL
            Undo.RecordObject(graph, "Create Node");
#endif
            var newNode = ScriptableObject.CreateInstance<T>();
            newNode.name = typeof(T).Name;
#if UNDO_EXPERIMENTAL
            Undo.RegisterCreatedObjectUndo(newNode, "Create Node");
#endif
            AssetDatabase.AddObjectToAsset(newNode, graph);
            newNode.SetGraph(graph);
            graph.nodes.Add(newNode);
            EditorUtility.SetDirty(graph);
            return newNode;
        }

        public static ForceNode DuplicateNode(this ForceGraph graph, ForceNode node)
        {
#if UNDO_EXPERIMENTAL
            Undo.RecordObject(graph, "Duplicate Node");
#endif
            var newNode = ScriptableObject.Instantiate(node);
            newNode.name = node.name;
#if UNDO_EXPERIMENTAL
            Undo.RegisterCreatedObjectUndo(newNode, "Duplicate Node");
#endif
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
#if UNDO_EXPERIMENTAL
            Undo.SetCurrentGroupName("Delete Node");
            int group = Undo.GetCurrentGroup();
            Undo.RecordObject(graph, "Delete Node");
#endif
            graph.nodes.Remove(node);
#if UNDO_EXPERIMENTAL
            Undo.DestroyObjectImmediate(node);
            Undo.CollapseUndoOperations(group);
#else
            ScriptableObject.DestroyImmediate(node, true);
#endif
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

#if UNDO_EXPERIMENTAL
            Undo.RecordObject(graph, "Create Connection");
#endif
            var newConnection = (ForceConnection)ScriptableObject.CreateInstance(type);
            newConnection.name = type.Name;
            newConnection.from = from;
            newConnection.to = to;
#if UNDO_EXPERIMENTAL
            Undo.RegisterCreatedObjectUndo(newConnection, "Create Connection");
#endif
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
#if UNDO_EXPERIMENTAL
            Undo.SetCurrentGroupName("Delete Connection");
            int group = Undo.GetCurrentGroup();
            Undo.RecordObject(graph, "Delete Connection");
#endif
            graph.connections.Remove(connection);
#if UNDO_EXPERIMENTAL
            Undo.DestroyObjectImmediate(connection);
            Undo.CollapseUndoOperations(group);
#else
            ScriptableObject.DestroyImmediate(connection, true);
#endif
            EditorUtility.SetDirty(graph);
        }

        public static ForceGroup CreateGroup(this ForceGraph graph, Type type)
        {
#if UNDO_EXPERIMENTAL
            Undo.RecordObject(graph, "Create Group");
#endif
            var group = (ForceGroup)ScriptableObject.CreateInstance(type);
            group.name = type.Name;
#if UNDO_EXPERIMENTAL
            Undo.RegisterCreatedObjectUndo(group, "Create Group");
#endif
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

#if UNDO_EXPERIMENTAL
            Undo.RecordObject(group, "Add Node to Group");
#endif
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
        public static void RemoveNodeFromAllGroups(this ForceGraph graph, ForceNode node)
        {
            foreach (var group in graph.groups)
            {
                if (group.nodes.Contains(node))
                {
#if UNDO_EXPERIMENTAL
                    Undo.RecordObject(group, "Remove Node from Group");
#endif
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

#if UNDO_EXPERIMENTAL
            Undo.SetCurrentGroupName("Delete Group");
            int g = Undo.GetCurrentGroup();
            Undo.RecordObject(graph, "Delete Group");
#endif
            graph.groups.Remove(group);
#if UNDO_EXPERIMENTAL
            Undo.DestroyObjectImmediate(group);
            Undo.CollapseUndoOperations(g);
#else
            ScriptableObject.DestroyImmediate(group, true);
#endif
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

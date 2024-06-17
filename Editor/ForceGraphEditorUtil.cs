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
            graph.nodes.Add(node);
            return node;
        }

        public static T CreateNode<T>(this ForceGraph graph) where T : ForceNode
        {
            var newNode = ScriptableObject.CreateInstance<T>();
            newNode.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(newNode, graph);
            graph.nodes.Add(newNode);
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
    }
}

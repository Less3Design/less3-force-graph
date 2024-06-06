using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;


namespace Less3.ForceGraph
{
[HideMonoScript]
public abstract class ForceGraph : ScriptableObject
{
    public List<ForceNode> nodes = new List<ForceNode>();
    public List<ForceConnection> connections = new List<ForceConnection>();

    public ForceNode CreateNode(Type type)
    {
        var node = (ForceNode)ScriptableObject.CreateInstance(type);
        node.name = type.Name;
        AssetDatabase.AddObjectToAsset(node, this);
        nodes.Add(node);
        return node;
    }

    public T CreateNode<T>() where T : ForceNode
    {
        var newNode = ScriptableObject.CreateInstance<T>();
        newNode.name = typeof(T).Name;
        AssetDatabase.AddObjectToAsset(newNode, this);
        nodes.Add(newNode);
        return newNode;
    }

    public void DeleteNode(ForceNode node)
    {
        if (nodes.Contains(node) == false)
        {
            Debug.LogError("Cannot delete node that is not in the graph");
            return;
        }
        nodes.Remove(node);
        DestroyImmediate(node, true);
    }

    public ForceConnection CreateConnection(ForceNode from, ForceNode to, Type type)
    {
        if (nodes.Contains(from) == false || nodes.Contains(to) == false)
        {
            Debug.LogError("Cannot create connection between nodes that are not in the graph");
            return null;
        }
        if (ValidateConnectionRequest(from, to, type) == false)
        {
            return null;
        }

        //TODO: validate if `type` is a child of ForceConnection

        var newConnection = (ForceConnection)ScriptableObject.CreateInstance(type);
        newConnection.name = type.Name;
        newConnection.from = from;
        newConnection.to = to;
        AssetDatabase.AddObjectToAsset(newConnection, this);
        connections.Add(newConnection);
        return newConnection;
    }

    public void DeleteConnection(ForceConnection connection)
    {
        if (connections.Contains(connection) == false)
        {
            Debug.LogError("Cannot delete connection that is not in the graph");
            return;
        }
        connections.Remove(connection);
        DestroyImmediate(connection, true);
    }

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

    public abstract List<Type> GraphNodeTypes();
    public abstract List<Type> GraphConnectionTypes();
}
}

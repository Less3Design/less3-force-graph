using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Less3.ForceGraph;

[CreateAssetMenu(fileName = "GenerationGraph", menuName = "GenerationGraph")]
public class GenerationGraph : ForceGraph
{

    public float test;
    public string customProperty;
    public string customProperty2;

    [ContextMenu("Create Node")]
    private void Create()
    {
        CreateNode<GenerationNode>();
    }

    public override List<Type> GraphNodeTypes()
    {
        return new List<Type>
        {
            typeof(GenerationNode)
        };
    }
    public override List<Type> GraphConnectionTypes()
    {
        return new List<Type>
        {
            typeof(GenerationConnection)
        };
    }
}

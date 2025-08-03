using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Less3.ForceGraph;

[CreateAssetMenu(fileName = "GenerationGraph", menuName = "Force Graphs/Generation Graph")]
public class GenerationGraph : ForceGraph
{
    public float test;
    public string customProperty;
    public string customProperty2;

    // ? Here I am defining what node types we want this graph to support.
    //   The types and strings below populate the create node menu in the editor.

    public override List<(string, Type)> GraphNodeTypes()
    {
        return new List<(string, Type)>
        {
            ("GenNode" , typeof(GenerationNode))
        };
    }

    public override List<(string, Type)> GraphGroupTypes()
    {
        return new List<(string, Type)>
        {
            ("GenGroup", typeof(GenerationGroup))
        };
    }

    public override Dictionary<Type, List<(string, Type)>> GraphConnectionTypes()
    {
        return new Dictionary<Type, List<(string, Type)>>
        {
            // The generation node...
            {typeof(GenerationNode), new List<(string,Type)>() {
                // Can be connected with these types
                ("Gen Connection", typeof(GenerationConnection))
            }}
        };
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Less3.ForceGraph;

public class GenerationGraph : ForceGraph
{

    public float test;
    public string customProperty;
    public string customProperty2;

    public override List<(string, Type)> GraphNodeTypes()
    {
        return new List<(string, Type)>
        {
            ("GenNode" , typeof(GenerationNode))
        };
    }
    public override Dictionary<Type, List<(string, Type)>> GraphConnectionTypes()
    {
        return new Dictionary<Type, List<(string, Type)>>
        {
            {typeof(GenerationNode), new List<(string,Type)>() {
                ("Gen Connection", typeof(GenerationConnection))
            }}
        };
    }
}

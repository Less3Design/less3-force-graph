using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Less3.ForceGraph;
using System.Diagnostics;

public class GenerationNode : ForceNode, IForceNodeIcon
{
    public string NodeIcon => ForceNodeIcons.Data;
    public Color NodeBackgroundColor => Color.green;
    public Color NodeLabelColor => Color.black;

    public float anotheRTest;
    public float otherTest;
}

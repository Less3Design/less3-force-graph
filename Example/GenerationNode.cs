using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Less3.ForceGraph;
using System.Diagnostics;

public class GenerationNode : ForceNode, IForceNodeIcon, IForceNodeTitle
{

    public string n;
    public string NodeTitle => n;
    public string NodeIcon => ForceNodeIcons.Data;
    public Color NodeBackgroundColor => Color.green;
    public Color NodeLabelColor => Color.black;
    public List<string> test = new List<string>();
    public List<GameObject> test2 = new List<GameObject>();

    public float anotheRTest;
    public float otherTest;
}

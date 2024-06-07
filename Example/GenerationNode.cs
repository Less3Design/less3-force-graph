using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Less3.ForceGraph;

public class GenerationNode : ForceNode , IForceNodeStyle
{
    public Color NodeBackgroundColor => Color.green;
    public Color NodeLabelColor => Color.black;

    public float anotheRTest;
    public float otherTest;

}

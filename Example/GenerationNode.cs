using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Less3.ForceGraph;

[DrawWithVisualElements]
public class GenerationNode : ForceNode , IForceNodeStyle
{
    public Color NodeBackgroundColor => Color.green;
    public Color NodeLabelColor => Color.black;

    [ReadOnly]
    public float anotheRTest;
    [ProgressBar(0, 100)]
    public float otherTest;

}

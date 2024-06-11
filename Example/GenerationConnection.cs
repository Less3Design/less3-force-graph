using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Less3.ForceGraph;

public class GenerationConnection : ForceConnection , IForceConnectionStyle
{
    public float test;
    public string customProperty;
    public string customProperty2;
    public float anotheRTest;
    public float otherTest;

    public Color ConnectionColor => Color.red;
    public bool Dashed => true;
}

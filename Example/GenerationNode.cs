using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Less3.ForceGraph;

public class GenerationNode : ForceNode, IForceNodeIcon, IForceNodeTitle, ILNodeEditorDoubleClick
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

    public void EditorOnNodeDoubleClick()
    {
        Debug.Log($"Double clicked on node: {n}");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Less3.ForceGraph;

public class GenerationNode : ForceNode, IForceNodeIcon, IForceNodeTitle, ILNodeEditorDoubleClick, IForceNodeSurTitle, IForceNodeSubTitle, IForceNodeBadges
{
    public string surTitle;
    public string title;
    public string subTitle;

    public string NodeTitle => title;
    public string NodeSurTitle => surTitle;
    public string NodeSubTitle => subTitle;

    public string NodeIcon => ForceNodeIcons.Data;
    public Color NodeBackgroundColor => Color.green;
    public Color NodeLabelColor => Color.black;
    public List<string> test = new List<string>();
    public List<GameObject> test2 = new List<GameObject>();

    public NodeBadges NodeBadges => showABadge;

    public float anotheRTest;
    public float otherTest;
    public NodeBadges showABadge;

    public void EditorOnNodeDoubleClick()
    {
        Debug.Log($"Double clicked on node: {title}");
    }
}

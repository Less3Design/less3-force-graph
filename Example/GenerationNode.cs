using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Less3.ForceGraph;

[LCreateNodeMenu(typeof(GenerationGraph), "Nodes/Example/GenNode")]
[LCreateNodeMenu(typeof(GenerationGraph), "Nodes/Example/GenNode2")]
[LCreateNodeMenu(typeof(GenerationGraph), "Nodes/Example/GenNode3")]
[LCreateNodeMenu(typeof(GenerationGraph), "Nodes/Example/GenNode4")]
[LCreateNodeMenu(typeof(GenerationGraph), "Nodes/Example/GenNode5")]
[LCreateNodeMenu(typeof(GenerationGraph), "Nodes/Example/GenNode6")]
[LCreateNodeMenu(typeof(GenerationGraph), "Nodes/Example/GenNode7")]
[LCreateNodeMenu(typeof(GenerationGraph), "Nodes/Example/GenNode8")]
[LCreateNodeMenu(typeof(GenerationGraph), "Nodes/Example/GenNode9")]
[LCreateNodeMenu(typeof(GenerationGraph), "Nodes/Example/GenNode10")]
[LCreateNodeMenu(typeof(GenerationGraph), "Nodes/Example/GenNode11")]

[LCreateNodeMenu(typeof(GenerationGraph), "Test")]
public class GenerationNode : ForceNode, IForceNodeIcon, IForceNodeTitle, ILNodeEditorDoubleClick, IForceNodeSurTitle, ILCanvasTags, IForceNodeBadges
{
    public string surTitle;
    public string title;
    public string subTitle;

    public bool dashConnections;

    public string NodeTitle => title;
    public string NodeSurTitle => surTitle;
    public List<LCanvasNodeTag> NodeTags
    {
        get
        {
            List<LCanvasNodeTag> tags = new List<LCanvasNodeTag>();
            if (!string.IsNullOrEmpty(subTitle))
            {
                tags.Add(new LCanvasNodeTag() { text = subTitle, tooltip = "This is a subtitle" });
            }
            if (test != null && test.Count > 0)
            {
                tags.Add(new LCanvasNodeTag() { text = $"Strings: {test.Count}", tooltip = "This is a list of strings" });
            }
            if (test2 != null && test2.Count > 0)
            {
                tags.Add(new LCanvasNodeTag() { text = $"GameObjects: {test2.Count}", tooltip = "This is a list of GameObjects" });
            }
            return tags;
        }
    }

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

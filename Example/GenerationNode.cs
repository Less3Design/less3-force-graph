#if LESS3_EXAMPLES
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Less3.Graph;

[L3CreateNodeMenu(typeof(GenerationGraph), "Test")]
public class GenerationNode : L3GraphNode
{
    public string surTitle;
    public string title;
    public string subTitle;

    public bool dashConnections;

    public string NodeTitle => title;
    public string NodeSurTitle => surTitle;
    public List<NodeTag> NodeTags
    {
        get
        {
            List<NodeTag> tags = new List<NodeTag>();
            if (!string.IsNullOrEmpty(subTitle))
            {
                tags.Add(new NodeTag() { text = subTitle, tooltip = "This is a subtitle" });
            }
            if (test != null && test.Count > 0)
            {
                tags.Add(new NodeTag() { text = $"Strings: {test.Count}", tooltip = "This is a list of strings" });
            }
            if (test2 != null && test2.Count > 0)
            {
                tags.Add(new NodeTag() { text = $"GameObjects: {test2.Count}", tooltip = "This is a list of GameObjects" });
            }
            return tags;
        }
    }

    public string NodeIcon => L3GraphNodeIcons.Data;
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
#endif

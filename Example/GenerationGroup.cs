using Less3.ForceGraph;
using UnityEngine;

public class GenerationGroup : ForceGroup, ILGroupLabel
{
    public float thisIsAGroupProperty;

    public string labelField = "New Group";
    public string Label
    {
        get => labelField;
    }
}

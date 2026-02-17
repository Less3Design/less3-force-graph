#if LESS3_EXAMPLES
using Less3.Graph;
using UnityEngine;

public class GenerationGroup : L3GraphGroup, IGroupLabel
{
    public float thisIsAGroupProperty;

    public string labelField = "New Group";
    public string Label
    {
        get => labelField;
    }
}
#endif

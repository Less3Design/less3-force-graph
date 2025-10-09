using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Less3.ForceGraph
{
    public abstract class ForceConnection : ScriptableObject
    {
        [HideInInspector]
        public ForceNode from;
        [HideInInspector]
        public ForceNode to;

        public static Color defaultColor = new Color(.7f, .7f, .7f, 1f);
    }
}

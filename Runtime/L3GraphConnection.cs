using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Less3.Graph
{
    public abstract class L3GraphConnection : ScriptableObject
    {
        [HideInInspector]
        public L3GraphNode from;
        [HideInInspector]
        public L3GraphNode to;

        public static Color defaultColor = new Color(.7f, .7f, .7f, 1f);
    }
}

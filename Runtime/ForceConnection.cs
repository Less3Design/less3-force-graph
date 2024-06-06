using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Less3.ForceGraph
{
    public abstract class ForceConnection : ScriptableObject
    {
        public ForceNode from;
        public ForceNode to;
    }
}

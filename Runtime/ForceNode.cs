using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Less3.ForceGraph
{
    public abstract class ForceNode : ScriptableObject
    {
        override public string ToString()
        {
            return name;
        }
    }
}

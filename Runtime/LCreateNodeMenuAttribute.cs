using System;
using UnityEngine;

namespace Less3.Graph
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class L3CreateNodeMenuAttribute : PropertyAttribute
    {
        public Type graphType;
        public string path;

        public L3CreateNodeMenuAttribute(Type graphType, string path)
        {
            this.graphType = graphType;
            this.path = path;
        }
    }
}

using System;
using UnityEngine;

namespace Less3.ForceGraph
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class LCreateNodeMenuAttribute : PropertyAttribute
    {
        public Type graphType;
        public string path;

        public LCreateNodeMenuAttribute(Type graphType, string path)
        {
            this.graphType = graphType;
            this.path = path;
        }
    }
}

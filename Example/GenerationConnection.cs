using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Less3.ForceGraph;

public class GenerationConnection : ForceConnection, IForceConnectionStyle, IForceConnectionIsDirectional
{
    public float test;
    public string customProperty;
    public string customProperty2;
    public float anotheRTest;
    public float otherTest;
    public bool directional;

    public Color ConnectionColor => ForceConnection.defaultColor;
    public bool Dashed => DashFunc();
    public bool IsDirectional => directional;

    bool DashFunc()
    {
        if (from is GenerationNode gn && gn.dashConnections)
        {
            return true;
        }
        else if (to is GenerationNode tn && tn.dashConnections)
        {
            return true;
        }
        return false;
    }
}
